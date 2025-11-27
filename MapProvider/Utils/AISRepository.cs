namespace MapProvider.Utils
{
    public class AISRepository
    {
        private readonly DBConnection _db;

        public AISRepository()
        {
            _db = new DBConnection();
        }

        // ===========================
        // Upsert Position Report
        // ===========================
        public async Task UpsertPositionReportAsync(PositionReport pos, AISMetaData meta)
        {
            using var conn = _db.GetConnection();

            // ------------------------------
            // 1. Upsert AIS metadata
            // ------------------------------
            const string upsertMeta = @"
                INSERT INTO ais_data (mmsi, mmsi_string, ship_name, latitude, longitude, time_utc)
                VALUES (@MMSI, @MMSI_String, @ShipName, @Latitude, @Longitude, @TimeUTC)
                ON CONFLICT (mmsi) DO UPDATE SET
                    mmsi_string = EXCLUDED.mmsi_string,
                    ship_name = EXCLUDED.ship_name,
                    latitude = EXCLUDED.latitude,
                    longitude = EXCLUDED.longitude,
                    time_utc = EXCLUDED.time_utc;
            ";

            DateTime utcTime;
            if (!string.IsNullOrWhiteSpace(meta.TimeUTC) &&
                DateTime.TryParse(meta.TimeUTC, out var parsedTime))
                utcTime = parsedTime;
            else
                utcTime = DateTime.UtcNow;

            await conn.ExecuteAsync(upsertMeta, new
            {
                meta.MMSI,
                meta.MMSI_String,
                meta.ShipName,
                meta.Latitude,
                meta.Longitude,
                TimeUTC = utcTime
            });

            // ------------------------------
            // 2. Create vessel ONLY IF NOT EXISTS
            // ------------------------------

            const string insertVesselIfNotExists = @"
                INSERT INTO vessels (mmsi, name)
                VALUES (@MMSI, @Name)
                ON CONFLICT (mmsi) DO NOTHING;
            ";

            // We only insert minimal vessel data.
            // Callsign, IMO, dimension, etc. should NEVER be updated here
            await conn.ExecuteAsync(insertVesselIfNotExists, new
            {
                MMSI = meta.MMSI,
                Name = meta.ShipName
            });

            // ------------------------------
            // 3. Insert Position Report (NO UPSERT)
            // ------------------------------

            const string insertPosition = @"
                INSERT INTO vessel_positions (
                    mmsi, latitude, longitude, cog, sog, trueheading,
                    rateofturn, navigationalstatus, positionaccuracy,
                    raim, valid, timestamp
                )
                VALUES (
                    @MMSI, @Latitude, @Longitude, @COG, @SOG, @TrueHeading,
                    @RateOfTurn, @NavigationalStatus, @PositionAccuracy,
                    @RAIM, @Valid, @Timestamp
                );
            ";

            await conn.ExecuteAsync(insertPosition, new
            {
                MMSI = meta.MMSI,
                pos.Latitude,
                pos.Longitude,
                pos.COG,
                pos.SOG,
                pos.TrueHeading,
                pos.RateOfTurn,
                pos.NavigationalStatus,
                pos.PositionAccuracy,
                RAIM = pos.RAIM,
                pos.Valid,
                Timestamp = pos.Timestamp
            });
        }


       public async Task UpsertShipStaticAsync(ShipStaticData ship, AISMetaData meta)
        {
            using var conn = _db.GetConnection();

            // ============================
            // 1. Upsert AIS Metadata
            // ============================
            const string upsertMeta = @"
                INSERT INTO ais_data (mmsi, mmsi_string, ship_name, latitude, longitude, time_utc)
                VALUES (@MMSI, @MMSI_String, @ShipName, @Latitude, @Longitude, @TimeUTC)
                ON CONFLICT (mmsi) DO UPDATE SET
                    mmsi_string = EXCLUDED.mmsi_string,
                    ship_name = EXCLUDED.ship_name,
                    latitude = EXCLUDED.latitude,
                    longitude = EXCLUDED.longitude,
                    time_utc = EXCLUDED.time_utc;
            ";

            DateTime parsedUtc;
            if (!string.IsNullOrWhiteSpace(meta.TimeUTC) &&
                DateTime.TryParse(meta.TimeUTC, out var parsed))
                parsedUtc = parsed;
            else
                parsedUtc = DateTime.UtcNow;

            await conn.ExecuteAsync(upsertMeta, new
            {
                meta.MMSI,
                meta.MMSI_String,
                meta.ShipName,
                meta.Latitude,
                meta.Longitude,
                TimeUTC = parsedUtc
            });

            // ============================
            // 2. Vessel upsert
            // ============================

            // --- Dimension JSON fix ---
            string dimensionJson = ship.Dimension != null
                ? JsonSerializer.Serialize(ship.Dimension)
                : null;

            // prevent "{}" when AIS dimension is all zeros
            if (dimensionJson == "{}")
                dimensionJson = null;

            // --- ETA JSON ---
            string etaJson = ship.Eta != null
                ? JsonSerializer.Serialize(ship.Eta)
                : null;

            const string upsertVessel = @"
                INSERT INTO vessels (
                    mmsi, name, callsign, destination, dimension, maximum_static_draught,
                    imo_number, type, eta
                )
                VALUES (
                    @MMSI,
                    @Name,
                    NULLIF(@CallSign, ''),              -- convert empty to NULL
                    @Destination,
                    NULLIF(@Dimension::jsonb, 'null'),  -- jsonb NULL guard
                    @MaximumStaticDraught,
                    NULLIF(@IMONumber, 0),              -- protect invalid IMO
                    @Type,
                    @ETA::jsonb
                )
                ON CONFLICT (mmsi) DO UPDATE SET
                    destination = EXCLUDED.destination,
                    maximum_static_draught = EXCLUDED.maximum_static_draught,
                    eta = EXCLUDED.eta,

                    -- update these ONLY if the new value is NOT NULL
                    callsign = COALESCE(EXCLUDED.callsign, vessels.callsign),
                    dimension = COALESCE(EXCLUDED.dimension, vessels.dimension),
                    imo_number = COALESCE(EXCLUDED.imo_number, vessels.imo_number);
            ";

            await conn.ExecuteAsync(upsertVessel, new
            {
                meta.MMSI,
                ship.Name,
                ship.CallSign,
                ship.Destination,
                Dimension = dimensionJson,        // <- cleaned JSON
                ship.MaximumStaticDraught,
                ship.IMONumber,
                ship.Type,
                ETA = etaJson                     // <- cleaned JSON
            });
        }

        
    }
}

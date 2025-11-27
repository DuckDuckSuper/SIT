-- ============================================================
-- 1. AIS metadata table
-- ============================================================
CREATE TABLE ais_data (
    mmsi BIGINT PRIMARY KEY,
    mmsi_string BIGINT NOT NULL,
    ship_name TEXT NOT NULL,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,
    time_utc TIMESTAMP WITH TIME ZONE NOT NULL
);

-- ============================================================
-- 2. Vessels static data table
-- ============================================================
CREATE TABLE vessels (
    mmsi BIGINT PRIMARY KEY,
    name TEXT NOT NULL,
    callsign TEXT,
    destination TEXT,
    dimension JSONB,                     -- JSON object {A,B,C,D}
    maximum_static_draught DOUBLE PRECISION,
    imo_number BIGINT,
    type INT,
    eta JSONB,                           -- JSON object {Day,Hour,Month,Minute}
    CONSTRAINT fk_ais_data FOREIGN KEY (mmsi)
        REFERENCES ais_data (mmsi)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

-- ============================================================
-- 3. Vessel position table (HISTORICAL TRACKING)
-- ============================================================
CREATE TABLE vessel_positions (
    id BIGSERIAL PRIMARY KEY,            -- unique row ID (required for history)

    mmsi BIGINT NOT NULL,                -- NOT UNIQUE → allows many rows per vessel
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,

    cog DOUBLE PRECISION,
    sog DOUBLE PRECISION,
    trueheading INT,
    rateofturn INT,
    navigationalstatus INT,
    positionaccuracy BOOLEAN,
    raim BOOLEAN,
    valid BOOLEAN,

    timestamp INT,                       -- Timestamp from AIS message (0–59)

    CONSTRAINT fk_vessel FOREIGN KEY (mmsi)
        REFERENCES vessels (mmsi)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

-- ============================================================
-- Recommended indexes for performance
-- ============================================================
CREATE INDEX idx_vessel_positions_mmsi ON vessel_positions (mmsi);
CREATE INDEX idx_vessel_positions_latlon ON vessel_positions (latitude, longitude);
CREATE INDEX idx_vessel_positions_timestamp ON vessel_positions (timestamp);

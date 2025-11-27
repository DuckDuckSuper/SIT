namespace MapProvider.Process
{
    public class VesselCollector
    {
        private readonly string _apiKey;
        private readonly string _apiUrl;

        private readonly AISRepository _aisRepository;
        public VesselCollector(string apiUrl, string apiKey)
        {
            _apiKey = apiKey;
            _apiUrl = apiUrl;
            
            _aisRepository = new AISRepository();
        }

        public async Task ReceiveAisMessagesAsync()
        {
            using var client = new ClientWebSocket();
            await client.ConnectAsync(new Uri(_apiUrl), CancellationToken.None);
            Console.WriteLine("Connected to AISStream.");

            // Subscribe to Singapore bounding box
            var subscription = new
            {
                APIKey = _apiKey,
                BoundingBoxes = new double[][][]
                {
                    new double[][]
                    {
                        new double[] { 1.13, 103.59 }, // SW corner
                        new double[] { 1.48, 104.09 }  // NE corner
                    }
                }
            };

            string subJson = JsonSerializer.Serialize(subscription);
            await client.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(subJson)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);

            Console.WriteLine("Subscription sent. Listening for messages...");

            var buffer = new byte[8192];

            while (client.State == WebSocketState.Open)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    Console.WriteLine("WebSocket closed.");
                    break;
                }

                try
                {
                    string decodedText = null;

                    // Decode text message
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        decodedText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    }
                    // Decode binary message (with optional decompression)
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        byte[] binaryData = buffer[..result.Count];
                        decodedText = Encoding.UTF8.GetString(binaryData);

                        if (!decodedText.TrimStart().StartsWith("{"))
                        {
                            using var ms = new MemoryStream(binaryData);
                            using var deflate = new DeflateStream(ms, CompressionMode.Decompress);
                            using var reader = new StreamReader(deflate, Encoding.UTF8);
                            decodedText = reader.ReadToEnd();
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(decodedText))
                    {
                        // Print raw JSON message
                        //Console.WriteLine("Decoded Message: " + decodedText);

                        // Deserialize into AISData
                        AISData aisData = JsonSerializer.Deserialize<AISData>(decodedText);

                        if (aisData != null)
                        {
                            // PositionReport
                            if (aisData.Message.PositionReport != null)
                            {
                                var pos = aisData.Message.PositionReport;
                                var meta = aisData.MetaData;

                                // Upsert position + metadata
                                await _aisRepository.UpsertPositionReportAsync(pos, meta);
                                Console.WriteLine("Vessel Position Updated: "+ meta.ShipName.TrimEnd()+"..."+" COG "+pos.COG+" SOG "+pos.SOG);  

                               // Console.WriteLine($"[{DateTime.UtcNow:O}] PositionReport: ShipID={pos.UserID}, " +
                                                //   $"Lat={pos.Latitude}, Lon={pos.Longitude}, " +
                                                //   $"SOG={pos.SOG}, COG={pos.COG}");
                            }
                            // ShipStaticData
                            else if (aisData.Message.ShipStaticData != null)
                            {
                                var ship = aisData.Message.ShipStaticData;
                                var meta = aisData.MetaData;

                                // Upsert static + metadata
                                await _aisRepository.UpsertShipStaticAsync(ship, meta);

                                Console.WriteLine($"[{DateTime.UtcNow:O}] ShipStaticData: ShipName={ship.Name}, " +
                                                  $"MMSI={ship.UserID}, Destination={ship.Destination}, " +
                                                  $"CallSign={ship.CallSign}, Type={ship.Type}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error decoding or parsing message: " + ex.Message);
                }
            }
        }
    }
}

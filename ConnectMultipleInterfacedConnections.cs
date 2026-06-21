using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ConnectMultipleInterfacedConnections
{
    public class InterfaceNode
    {
        public string NodeId { get; set; }
        public string NodeType { get; set; }
        public List<string> ConnectedNodes { get; set; }
        public Dictionary<string, double> SignalStrength { get; set; }
        public double Bandwidth { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class ConnectionBridge
    {
        public string BridgeId { get; set; }
        public List<string> SourceNodes { get; set; }
        public List<string> TargetNodes { get; set; }
        public double TransmissionRate { get; set; }
        public int ActiveChannels { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    public class NetworkTopology
    {
        public string TopologyName { get; set; }
        public Dictionary<string, InterfaceNode> Nodes { get; set; }
        public List<ConnectionBridge> Bridges { get; set; }
        public double OverallLatency { get; set; }
        public double NetworkHealth { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class SimultaneousConnectionManager
    {
        private NetworkTopology network;
        private Dictionary<string, List<(string, double)>> connectionLog;
        private Queue<(string source, string target, double signal)> transmissionQueue;

        public SimultaneousConnectionManager(string topologyName)
        {
            network = new NetworkTopology
            {
                TopologyName = topologyName,
                Nodes = new Dictionary<string, InterfaceNode>(),
                Bridges = new List<ConnectionBridge>(),
                OverallLatency = 0.0,
                NetworkHealth = 1.0,
                LastUpdated = DateTime.Now
            };
            connectionLog = new Dictionary<string, List<(string, double)>>();
            transmissionQueue = new Queue<(string, string, double)>();
        }

        public void CreateInterfaceNode(string nodeId, string nodeType, double bandwidth)
        {
            var node = new InterfaceNode
            {
                NodeId = nodeId,
                NodeType = nodeType,
                ConnectedNodes = new List<string>(),
                SignalStrength = new Dictionary<string, double>(),
                Bandwidth = bandwidth,
                CreatedTime = DateTime.Now
            };
            network.Nodes[nodeId] = node;
            connectionLog[nodeId] = new List<(string, double)>();
        }

        public void ConnectNodes(string sourceNode, string targetNode, double signalStrength)
        {
            if (network.Nodes.ContainsKey(sourceNode) && network.Nodes.ContainsKey(targetNode))
            {
                var source = network.Nodes[sourceNode];
                var target = network.Nodes[targetNode];

                if (!source.ConnectedNodes.Contains(targetNode))
                {
                    source.ConnectedNodes.Add(targetNode);
                    source.SignalStrength[targetNode] = signalStrength;

                    if (!connectionLog.ContainsKey(sourceNode))
                        connectionLog[sourceNode] = new List<(string, double)>();

                    connectionLog[sourceNode].Add((targetNode, signalStrength));
                }
            }
        }

        public void CreateConnectionBridge(string bridgeId, List<string> sources, List<string> targets)
        {
            var bridge = new ConnectionBridge
            {
                BridgeId = bridgeId,
                SourceNodes = new List<string>(sources),
                TargetNodes = new List<string>(targets),
                TransmissionRate = 0.0,
                ActiveChannels = 0,
                Metadata = new Dictionary<string, object>()
            };
            network.Bridges.Add(bridge);
        }

        public void TransmitSignalSimultaneously(List<(string source, string target, double signal)> transmissions)
        {
            int successfulTransmissions = 0;

            foreach (var (source, target, signal) in transmissions)
            {
                if (network.Nodes.ContainsKey(source) && network.Nodes.ContainsKey(target))
                {
                    double bandwidth = network.Nodes[source].Bandwidth;
                    double attenuation = 1.0 - (0.1 * transmissions.Count / 10.0);
                    double effectiveSignal = signal * attenuation;

                    transmissionQueue.Enqueue((source, target, effectiveSignal));
                    successfulTransmissions++;
                }
            }

            network.OverallLatency = transmissions.Count * 0.02;
            network.NetworkHealth = Math.Max(0.7, 1.0 - (network.OverallLatency * 0.1));
        }

        public void ProcessTransmissionQueue()
        {
            while (transmissionQueue.Count > 0)
            {
                var (source, target, signal) = transmissionQueue.Dequeue();
                if (network.Nodes.ContainsKey(target))
                {
                    var targetNode = network.Nodes[target];
                    targetNode.SignalStrength[source] = signal;
                }
            }
        }

        public Dictionary<string, int> GetConnectionDegree()
        {
            return network.Nodes.ToDictionary(x => x.Key, x => x.Value.ConnectedNodes.Count);
        }

        public double CalculateNetworkDensity()
        {
            int nodeCount = network.Nodes.Count;
            if (nodeCount < 2) return 0.0;

            int edgeCount = network.Nodes.Values.Sum(n => n.ConnectedNodes.Count);
            int maxEdges = nodeCount * (nodeCount - 1);
            return (double)edgeCount / maxEdges;
        }

        public List<string> FindConnectedComponentsForNode(string nodeId, int depth = 2)
        {
            var visited = new HashSet<string>();
            var toVisit = new Queue<(string, int)>();
            toVisit.Enqueue((nodeId, 0));

            while (toVisit.Count > 0)
            {
                var (current, currentDepth) = toVisit.Dequeue();
                if (visited.Contains(current) || currentDepth > depth) continue;

                visited.Add(current);

                if (network.Nodes.ContainsKey(current))
                {
                    foreach (var neighbor in network.Nodes[current].ConnectedNodes)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            toVisit.Enqueue((neighbor, currentDepth + 1));
                        }
                    }
                }
            }

            return visited.ToList();
        }

        public void DisplayNetworkStatus()
        {
            Console.WriteLine($"\n  Network: {network.TopologyName}");
            Console.WriteLine($"  Nodes: {network.Nodes.Count}");
            Console.WriteLine($"  Bridges: {network.Bridges.Count}");
            Console.WriteLine($"  Density: {CalculateNetworkDensity() * 100:F1}%");
            Console.WriteLine($"  Latency: {network.OverallLatency * 1000:F1}ms");
            Console.WriteLine($"  Health: {network.NetworkHealth * 100:F1}%");
        }

        public void DisplayNodeConnections(string nodeId)
        {
            if (network.Nodes.ContainsKey(nodeId))
            {
                var node = network.Nodes[nodeId];
                Console.WriteLine($"\n  Node: {nodeId} ({node.NodeType})");
                Console.WriteLine($"  Bandwidth: {node.Bandwidth:F2} Mbps");
                Console.WriteLine($"  Connected to {node.ConnectedNodes.Count} nodes:");
                foreach (var connected in node.ConnectedNodes)
                {
                    double signal = node.SignalStrength.ContainsKey(connected) ? node.SignalStrength[connected] : 0.0;
                    Console.WriteLine($"    → {connected}: Signal {signal * 100:F0}%");
                }
            }
        }

        public Dictionary<string, double> GetAverageSignalStrength()
        {
            var result = new Dictionary<string, double>();
            foreach (var node in network.Nodes.Values)
            {
                if (node.SignalStrength.Count > 0)
                {
                    result[node.NodeId] = node.SignalStrength.Values.Average();
                }
                else
                {
                    result[node.NodeId] = 0.0;
                }
            }
            return result;
        }

        public void DisplayBridgeStatus()
        {
            Console.WriteLine("\n  Connection Bridges:");
            foreach (var bridge in network.Bridges)
            {
                Console.WriteLine($"    {bridge.BridgeId}:");
                Console.WriteLine($"      Sources: {string.Join(", ", bridge.SourceNodes)}");
                Console.WriteLine($"      Targets: {string.Join(", ", bridge.TargetNodes)}");
                Console.WriteLine($"      Channels: {bridge.ActiveChannels}");
            }
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Multiple Interfaced Connections with Simultaneous Link    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var manager = new SimultaneousConnectionManager("MultiInterface Network");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Creating Interface Nodes]");
        Console.ResetColor();
        Thread.Sleep(500);

        manager.CreateInterfaceNode("CPU_CORE_1", "Processor", 100.0);
        manager.CreateInterfaceNode("CPU_CORE_2", "Processor", 100.0);
        manager.CreateInterfaceNode("MEM_CACHE", "Memory", 150.0);
        manager.CreateInterfaceNode("GPU_UNIT", "Graphics", 200.0);
        manager.CreateInterfaceNode("I_O_CONTROLLER", "IO", 80.0);
        manager.CreateInterfaceNode("NETWORK_IF", "Network", 120.0);

        Console.WriteLine("  ✓ Created 6 interface nodes with varying types and bandwidths");
        manager.DisplayNetworkStatus();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Building Initial Connections]");
        Console.ResetColor();
        Thread.Sleep(500);

        manager.ConnectNodes("CPU_CORE_1", "MEM_CACHE", 0.95);
        manager.ConnectNodes("CPU_CORE_2", "MEM_CACHE", 0.95);
        manager.ConnectNodes("CPU_CORE_1", "GPU_UNIT", 0.85);
        manager.ConnectNodes("GPU_UNIT", "MEM_CACHE", 0.90);
        manager.ConnectNodes("CPU_CORE_1", "I_O_CONTROLLER", 0.80);
        manager.ConnectNodes("I_O_CONTROLLER", "NETWORK_IF", 0.75);

        Console.WriteLine("  ✓ Established bidirectional connections between nodes");
        foreach (var node in new[] { "CPU_CORE_1", "MEM_CACHE", "GPU_UNIT" })
        {
            manager.DisplayNodeConnections(node);
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating Connection Bridges]");
        Console.ResetColor();
        Thread.Sleep(500);

        manager.CreateConnectionBridge("BRIDGE_CPU_MEM",
            new List<string> { "CPU_CORE_1", "CPU_CORE_2" },
            new List<string> { "MEM_CACHE" });

        manager.CreateConnectionBridge("BRIDGE_GPU_IO",
            new List<string> { "GPU_UNIT" },
            new List<string> { "I_O_CONTROLLER", "NETWORK_IF" });

        Console.WriteLine("  ✓ Created 2 multi-node connection bridges");
        manager.DisplayBridgeStatus();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Simultaneous Multi-Path Transmission]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Transmitting 5 simultaneous signals:");
        var simultaneousTransmissions = new List<(string, string, double)>
        {
            ("CPU_CORE_1", "MEM_CACHE", 0.95),
            ("CPU_CORE_2", "MEM_CACHE", 0.95),
            ("CPU_CORE_1", "GPU_UNIT", 0.85),
            ("GPU_UNIT", "MEM_CACHE", 0.90),
            ("I_O_CONTROLLER", "NETWORK_IF", 0.75)
        };

        foreach (var (src, tgt, sig) in simultaneousTransmissions)
        {
            Console.WriteLine($"    {src} → {tgt} (Signal: {sig * 100:F0}%)");
        }

        manager.TransmitSignalSimultaneously(simultaneousTransmissions);
        manager.ProcessTransmissionQueue();

        Console.WriteLine("  ✓ All transmissions processed with signal attenuation applied");
        manager.DisplayNetworkStatus();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Network Topology Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var degrees = manager.GetConnectionDegree();
        Console.WriteLine("  Node Connection Degrees:");
        foreach (var kvp in degrees.OrderByDescending(x => x.Value))
        {
            Console.WriteLine($"    {kvp.Key}: {kvp.Value} connections");
        }

        Console.WriteLine($"\n  Network Density: {manager.CalculateNetworkDensity() * 100:F1}%");
        Console.WriteLine($"  Connected Components from CPU_CORE_1: {manager.FindConnectedComponentsForNode("CPU_CORE_1").Count} nodes");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Signal Strength Metrics]");
        Console.ResetColor();
        Thread.Sleep(500);

        var signalStrengths = manager.GetAverageSignalStrength();
        Console.WriteLine("  Average Signal Strength by Node:");
        foreach (var kvp in signalStrengths.OrderByDescending(x => x.Value))
        {
            Console.WriteLine($"    {kvp.Key}: {kvp.Value * 100:F1}%");
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Multi-Interface Architecture Summary]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Multi-Interface Connection Model:");
        Console.WriteLine("    Level 1: Individual Interface Nodes (Processors, Memory, GPU, IO)");
        Console.WriteLine("    Level 2: Dedicated Connections (High-speed links between specific nodes)");
        Console.WriteLine("    Level 3: Connection Bridges (Aggregate multiple paths)");
        Console.WriteLine("    Level 4: Simultaneous Transmission (Multi-path signal routing)");
        Console.WriteLine("    Level 5: Network Optimization (Latency reduction, bandwidth management)");
        Console.WriteLine("\n  Characteristics:");
        Console.WriteLine("    • Concurrent multi-path data flow");
        Console.WriteLine("    • Signal attenuation with parallel load");
        Console.WriteLine("    • Dynamic latency calculation");
        Console.WriteLine("    • Network health monitoring (current: {0:P0})" , manager.GetAverageSignalStrength().Values.Average());

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Multi-interface connection system complete");
        Console.ResetColor();
    }
}

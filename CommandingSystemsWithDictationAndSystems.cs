using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class CommandingSystemsWithDictationAndSystems
{
    public class VoiceCommand
    {
        public string CommandId { get; set; }
        public string SpokenText { get; set; }
        public string InterpretedCommand { get; set; }
        public List<string> Parameters { get; set; }
        public double ConfidenceScore { get; set; }
        public DateTime ReceivedDate { get; set; }
    }

    public class SystemTarget
    {
        public string TargetId { get; set; }
        public string SystemName { get; set; }
        public string Description { get; set; }
        public List<string> AvailableCommands { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CommandDefinition
    {
        public string CommandId { get; set; }
        public string CommandName { get; set; }
        public List<string> ExpectedParameters { get; set; }
        public string ActionDescription { get; set; }
        public double ExecutionPriority { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ExecutionLog
    {
        public string LogId { get; set; }
        public string CommandId { get; set; }
        public string TargetSystem { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
        public double ExecutionTime { get; set; }
        public DateTime ExecutedDate { get; set; }
    }

    public class CommandingAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalCommandsReceived { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public double AverageConfidence { get; set; }
        public Dictionary<string, int> CommandFrequency { get; set; }
        public double AverageExecutionTime { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class CommandingEngine
    {
        private Dictionary<string, VoiceCommand> commands;
        private Dictionary<string, SystemTarget> systems;
        private Dictionary<string, CommandDefinition> definitions;
        private Dictionary<string, ExecutionLog> logs;
        private Dictionary<string, CommandingAnalysis> analyses;

        public CommandingEngine()
        {
            commands = new Dictionary<string, VoiceCommand>();
            systems = new Dictionary<string, SystemTarget>();
            definitions = new Dictionary<string, CommandDefinition>();
            logs = new Dictionary<string, ExecutionLog>();
            analyses = new Dictionary<string, CommandingAnalysis>();
        }

        public void RegisterSystem(string systemId, string systemName, string description, List<string> commands)
        {
            var system = new SystemTarget
            {
                TargetId = systemId,
                SystemName = systemName,
                Description = description,
                AvailableCommands = new List<string>(commands),
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            systems[systemId] = system;
        }

        public void RegisterCommandDefinition(string commandId, string commandName, List<string> parameters,
                                             string actionDesc, double priority)
        {
            var definition = new CommandDefinition
            {
                CommandId = commandId,
                CommandName = commandName,
                ExpectedParameters = new List<string>(parameters),
                ActionDescription = actionDesc,
                ExecutionPriority = priority,
                CreatedDate = DateTime.Now
            };
            definitions[commandId] = definition;
        }

        public void ProcessVoiceCommand(string commandId, string spokenText, List<string> parameters)
        {
            var command = new VoiceCommand
            {
                CommandId = commandId,
                SpokenText = spokenText,
                InterpretedCommand = InterpretCommand(spokenText),
                Parameters = new List<string>(parameters),
                ConfidenceScore = CalculateConfidence(spokenText, parameters),
                ReceivedDate = DateTime.Now
            };
            commands[commandId] = command;
        }

        private string InterpretCommand(string spokenText)
        {
            if (spokenText.Contains("activate") || spokenText.Contains("enable"))
                return "ACTIVATE";
            if (spokenText.Contains("deactivate") || spokenText.Contains("disable"))
                return "DEACTIVATE";
            if (spokenText.Contains("execute") || spokenText.Contains("run"))
                return "EXECUTE";
            if (spokenText.Contains("stop") || spokenText.Contains("halt"))
                return "STOP";
            if (spokenText.Contains("status"))
                return "STATUS";
            return "UNKNOWN";
        }

        private double CalculateConfidence(string spokenText, List<string> parameters)
        {
            double confidence = 0.8;
            if (spokenText.Length > 10) confidence += 0.1;
            if (parameters.Count >= 2) confidence += 0.05;
            return Math.Min(confidence, 1.0);
        }

        public void ExecuteCommand(string executionId, string commandId, string targetSystemId)
        {
            if (!commands.ContainsKey(commandId)) return;

            var voiceCmd = commands[commandId];
            var targetSystem = systems.ContainsKey(targetSystemId) ? systems[targetSystemId] : null;

            var log = new ExecutionLog
            {
                LogId = executionId,
                CommandId = commandId,
                TargetSystem = targetSystemId,
                Status = targetSystem?.IsActive == true ? "EXECUTED" : "FAILED",
                Result = GenerateExecutionResult(voiceCmd.InterpretedCommand),
                ExecutionTime = GenerateExecutionTime(),
                ExecutedDate = DateTime.Now
            };

            logs[executionId] = log;
        }

        private string GenerateExecutionResult(string command)
        {
            return $"Command '{command}' processed successfully";
        }

        private double GenerateExecutionTime()
        {
            return Math.Round(new Random().NextDouble() * 1000, 2);
        }

        public void AnalyzeCommandingPatterns(string analysisId)
        {
            var analysis = new CommandingAnalysis
            {
                AnalysisId = analysisId,
                TotalCommandsReceived = commands.Count,
                SuccessfulExecutions = logs.Count(l => l.Value.Status == "EXECUTED"),
                FailedExecutions = logs.Count(l => l.Value.Status == "FAILED"),
                AverageConfidence = commands.Count > 0 ? commands.Values.Average(c => c.ConfidenceScore) : 0.0,
                CommandFrequency = new Dictionary<string, int>(),
                AverageExecutionTime = logs.Count > 0 ? logs.Values.Average(l => l.ExecutionTime) : 0.0,
                AnalyzedDate = DateTime.Now
            };

            foreach (var cmd in commands.Values)
            {
                if (!analysis.CommandFrequency.ContainsKey(cmd.InterpretedCommand))
                    analysis.CommandFrequency[cmd.InterpretedCommand] = 0;
                analysis.CommandFrequency[cmd.InterpretedCommand]++;
            }

            analyses[analysisId] = analysis;
        }

        public void DisplayCommand(string commandId)
        {
            if (!commands.ContainsKey(commandId)) return;

            var cmd = commands[commandId];
            Console.WriteLine($"\n  Voice Command: {cmd.CommandId}");
            Console.WriteLine($"  Spoken: {cmd.SpokenText}");
            Console.WriteLine($"  Interpreted: {cmd.InterpretedCommand}");
            Console.WriteLine($"  Parameters: {string.Join(", ", cmd.Parameters)}");
            Console.WriteLine($"  Confidence: {cmd.ConfidenceScore * 100:F0}%");
        }

        public void DisplaySystem(string systemId)
        {
            if (!systems.ContainsKey(systemId)) return;

            var system = systems[systemId];
            Console.WriteLine($"\n  System: {system.SystemName}");
            Console.WriteLine($"  Status: {(system.IsActive ? "Active" : "Inactive")}");
            Console.WriteLine($"  Available Commands: {string.Join(", ", system.AvailableCommands)}");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Commanding Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Commands: {analysis.TotalCommandsReceived}");
            Console.WriteLine($"  Successful: {analysis.SuccessfulExecutions}");
            Console.WriteLine($"  Failed: {analysis.FailedExecutions}");
            Console.WriteLine($"  Average Confidence: {analysis.AverageConfidence * 100:F0}%");
            Console.WriteLine($"  Average Execution Time: {analysis.AverageExecutionTime:F0}ms");
        }

        public int GetTotalCommands()
        {
            return commands.Count;
        }

        public int GetTotalSystems()
        {
            return systems.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Commanding Systems with Dictation and Voice Control        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new CommandingEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering System Targets]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterSystem("SYS-001", "Process Manager",
            "Manages system processes and applications",
            new List<string> { "ACTIVATE", "DEACTIVATE", "STATUS", "EXECUTE" });

        engine.RegisterSystem("SYS-002", "Data Processing",
            "Processes data and generates reports",
            new List<string> { "EXECUTE", "CANCEL", "STATUS", "LOG" });

        Console.WriteLine("  ✓ Registered 2 system targets");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Registering Command Definitions]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterCommandDefinition("CMD-001", "Activate System",
            new List<string> { "SystemId" },
            "Activates a system target", 0.95);

        engine.RegisterCommandDefinition("CMD-002", "Execute Task",
            new List<string> { "TaskName", "Parameters" },
            "Executes a named task", 0.90);

        engine.RegisterCommandDefinition("CMD-003", "Get Status",
            new List<string> { "SystemId" },
            "Retrieves system status", 0.85);

        Console.WriteLine("  ✓ Registered 3 command definitions");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Processing Voice Commands]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.ProcessVoiceCommand("VOICE-001", "Activate system one",
            new List<string> { "SYS-001" });

        engine.ProcessVoiceCommand("VOICE-002", "Execute the data processing task",
            new List<string> { "DataProcess", "Priority=High" });

        engine.ProcessVoiceCommand("VOICE-003", "What is the status",
            new List<string> { "SYS-002" });

        Console.WriteLine("  ✓ Processed 3 voice commands");
        engine.DisplayCommand("VOICE-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Displaying System Targets]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplaySystem("SYS-001");
        engine.DisplaySystem("SYS-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Executing Commands]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.ExecuteCommand("EXEC-001", "VOICE-001", "SYS-001");
        engine.ExecuteCommand("EXEC-002", "VOICE-002", "SYS-002");
        engine.ExecuteCommand("EXEC-003", "VOICE-003", "SYS-002");

        Console.WriteLine("  ✓ Executed 3 commands on target systems");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Analyzing Commanding Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeCommandingPatterns("ANALYSIS-001");
        engine.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Voice Command System Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Voice Command Architecture:");
        Console.WriteLine("    Layer 1: System Registration (define target systems)");
        Console.WriteLine("    Layer 2: Command Definition (specify available commands)");
        Console.WriteLine("    Layer 3: Voice Input Processing (interpret spoken text)");
        Console.WriteLine("    Layer 4: Parameter Extraction (parse command parameters)");
        Console.WriteLine("    Layer 5: Confidence Scoring (measure interpretation accuracy)");
        Console.WriteLine("    Layer 6: Command Execution (trigger system actions)");
        Console.WriteLine("    Layer 7: Pattern Analysis (track command usage)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Register multiple system targets");
        Console.WriteLine("    ✓ Define command sets for systems");
        Console.WriteLine("    ✓ Process natural language voice input");
        Console.WriteLine("    ✓ Interpret and parse commands");
        Console.WriteLine("    ✓ Calculate confidence scores");
        Console.WriteLine("    ✓ Execute commands on target systems");
        Console.WriteLine("    ✓ Track execution results");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Voice commanding system complete");
        Console.ResetColor();
    }
}

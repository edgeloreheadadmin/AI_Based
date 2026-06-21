# Biorhythmic Decision-Making Engine

A sophisticated C# application that uses biorhythm calculations to provide data-driven recommendations for optimal decision-making timing.

## Overview

This system implements the biorhythm theory which suggests that human performance follows three cyclical patterns:

- **Physical Cycle (23 days)**: Energy levels, strength, endurance, physical coordination
- **Emotional Cycle (28 days)**: Mood, motivation, creativity, emotional stability
- **Intellectual Cycle (33 days)**: Logic, concentration, problem-solving ability

The engine calculates where you are in each cycle and provides recommendations for various types of decisions based on your biorhythmic alignment.

## Features

### Core Functionality

- **Biorhythm Calculation**: Computes sine-wave based biorhythm values for any date
- **Cycle Phase Detection**: Identifies current phase (high, low, transition, critical)
- **Decision Recommendations**: Tailored advice for 6 decision types:
  - Athletic (physical activities)
  - Creative (artistic/innovative work)
  - Analytical (logical/technical work)
  - Business (professional/strategic decisions)
  - Interpersonal (social/relationship decisions)
  - Health (medical/wellness decisions)

### Advanced Features

- **Adaptive Learning**: Records decision outcomes and adjusts recommendations based on historical performance
- **Performance Analytics**: Tracks success rates and optimal timing patterns
- **Critical Day Prediction**: Identifies high-risk days to avoid major decisions
- **Optimal Day Finding**: Predicts the best upcoming days for specific activities
- **CSV Export**: Export decision history for external analysis

## Architecture

### Core Classes

#### `BiorhythmCalculator`
Calculates biorhythm values using sinusoidal functions.

```csharp
var calculator = new BiorhythmCalculator(birthDate);
var profile = calculator.GetProfile(DateTime.Now);
```

#### `BiorhythmicDecisionEngine`
Provides decision recommendations based on biorhythmic alignment.

```csharp
var engine = new BiorhythmicDecisionEngine(birthDate);
var recommendation = engine.GetDecisionRecommendation("athletic", DateTime.Now);
```

#### `BiorhythmDecisionLearner`
Learns from past decisions and provides adaptive recommendations.

```csharp
var learner = new BiorhythmDecisionLearner(engine);
learner.RecordDecision("business", date, "success", 8, "Successful presentation");
var adaptive = learner.GetAdaptiveRecommendation("business", DateTime.Now);
```

#### `BiorhythmAnalytics`
Provides analytical insights and predictions.

```csharp
var analytics = new BiorhythmAnalytics(birthDate);
var optimalDays = analytics.FindOptimalDays("creative", 30);
var criticalDays = analytics.FindCriticalDays(30);
```

## Usage

### Basic Usage

```csharp
var birthDate = new DateTime(1990, 5, 15);
var engine = new BiorhythmicDecisionEngine(birthDate);

// Get recommendation for today
var recommendation = engine.GetDecisionRecommendation("athletic", DateTime.Now);
Console.WriteLine($"Score: {recommendation.RecommendationScore}/100");
Console.WriteLine($"Recommendation: {recommendation.Recommendation}");
```

### Recording Decisions

```csharp
var learner = new BiorhythmDecisionLearner(engine);

// After making and evaluating a decision
learner.RecordDecision(
    decisionType: "business",
    decisionDate: new DateTime(2024, 6, 15),
    outcome: "success",
    rating: 9,
    notes: "Project approval meeting went exceptionally well"
);
```

### Analytics

```csharp
var analytics = new BiorhythmAnalytics(birthDate);

// Find best days in next 60 days for creative work
var optimalDays = analytics.FindOptimalDays("creative", 60);
foreach (var (date, score) in optimalDays)
{
    Console.WriteLine($"{date:MMM dd} - Score: {score:F1}/100");
}

// Identify critical days to avoid
var criticalDays = analytics.FindCriticalDays(30);
```

## Scoring System

### Recommendation Score (0-100)

- **80-100**: ✅ Highly Recommended - Excellent conditions
- **60-79**: ✓ Recommended - Good conditions
- **40-59**: ⚠️ Proceed with Caution - Suboptimal conditions
- **0-39**: ❌ Not Recommended - Poor alignment

### Cycle Phases

- **HighPhase** (50-100% of cycle): Peak performance capability
- **TransitionHigh** (positive values declining): Decreasing capability
- **Critical** (±10% of zero crossing): Unstable, avoid major decisions
- **TransitionLow** (negative values, increasing): Building to low phase
- **LowPhase** (0-50% of cycle): Minimum performance capability

## Data Persistence

Decision records are automatically saved to `decision_history.json`:

```json
[
  {
    "DecisionDate": "2024-06-15T00:00:00",
    "DecisionType": "business",
    "BiorhythmScore": 78.5,
    "Outcome": "success",
    "OutcomeRating": 9,
    "Notes": "Project approval meeting"
  }
]
```

Export to CSV:
```csharp
var csv = learner.ExportToCsv();
File.WriteAllText("decisions.csv", csv);
```

## Mathematical Foundation

### Biorhythm Calculation

Each cycle is calculated using a sinusoidal wave:

```
Value = sin(2π × (days_since_birth % cycle_length) / cycle_length)
```

Where:
- Days since birth: calculated from birth date to target date
- Cycle length: 23 (physical), 28 (emotional), or 33 (intellectual)
- Result range: -1.0 to +1.0

### Harmonic Index

Overall alignment score combining all three cycles:

```
HarmonicIndex = (Physical + Emotional + Intellectual) / 3 + 1) / 2
Result range: 0.0 to 1.0
```

## Performance Optimization

The system uses:
- In-memory caching of historical decisions
- Lazy calculation of future optimal days
- Efficient JSON serialization for data persistence

## Building and Running

### Prerequisites
- .NET 8.0 or higher
- C# 11.0 or higher

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run
```

### Interactive Menu

The application provides an interactive menu with options to:
1. Get instant recommendations
2. Access adaptive recommendations based on history
3. Record decision outcomes
4. View performance statistics
5. Find optimal days
6. Identify critical days
7. View detailed phase analysis
8. Get comprehensive daily analysis

## Limitations & Considerations

1. **Theory Status**: Biorhythm theory is pseudoscientific; use as a decision support tool, not the sole basis for important decisions
2. **Accuracy**: Recommendations are probabilistic based on historical data
3. **Individual Variation**: Results may vary significantly between individuals
4. **Minimum History**: At least 3 recorded decisions recommended for adaptive learning
5. **Birth Date Accuracy**: Precise birth date required for accurate calculations

## Extension Points

- Add different decision category types
- Implement machine learning models for pattern recognition
- Create web API for remote access
- Add calendar visualization
- Integrate with calendar applications
- Support timezone variations
- Add meditation/reflection reminders

## License

This project is provided as-is for educational and experimental purposes.

## Contributing

Contributions welcome! Areas for enhancement:
- Advanced statistical analysis
- Machine learning integration
- Web interface
- Mobile app support
- Additional cycle types or custom cycles
- Real-time alerting system

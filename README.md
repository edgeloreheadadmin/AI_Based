# Automatic Biorhythmic Decision-Making Engine with Mindset Alignment

A sophisticated C# application that combines biorhythm calculations with automatic psychological state detection to provide intelligent, context-aware recommendations for optimal decision-making and activity scheduling.

## Overview

This advanced system implements two integrated subsystems:

### 1. Biorhythmic Alignment
Leverages the biorhythm theory which suggests human performance follows three cyclical patterns:
- **Physical Cycle (23 days)**: Energy levels, strength, endurance, physical coordination
- **Emotional Cycle (28 days)**: Mood, motivation, creativity, emotional stability
- **Intellectual Cycle (33 days)**: Logic, concentration, problem-solving ability

### 2. Automatic Mindset Alignment (NEW!)
Maps biorhythm cycles to 10 distinct psychological mindset types and automatically aligns decisions with your current mental state:
- **AnalyticalLogical**: Clear thinking, problem-focused
- **CreativeExpressive**: Imaginative, expressive, intuitive
- **ActionOriented**: Energetic, decisive, proactive
- **EmotionalIntuitive**: Empathetic, relationship-focused
- **BalancedHarmonious**: Centered, integrated (rare optimal state)
- **LowEnergyReflective**: Introspective, restorative
- **StressedFractured**: Conflicted, fragmented (avoid major decisions)
- **FocusedDetermined**: Concentrated, driven, intense
- **PlayfulSpontaneous**: Lighthearted, spontaneous, dynamic
- **CautiousConservative**: Risk-averse, protective

The system intelligently detects your current psychological state and recommends which activities, decisions, and work types you'll excel at RIGHT NOW.

## Features

### Core Biorhythm Functionality

- **Biorhythm Calculation**: Computes sine-wave based biorhythm values for any date
- **Cycle Phase Detection**: Identifies current phase (high, low, transition, critical)
- **Decision Recommendations**: Tailored advice for 6 decision types:
  - Athletic (physical activities)
  - Creative (artistic/innovative work)
  - Analytical (logical/technical work)
  - Business (professional/strategic decisions)
  - Interpersonal (social/relationship decisions)
  - Health (medical/wellness decisions)

### NEW: Automatic Mindset Alignment

- **10 Distinct Mindset Types**: Automatically detects your current psychological state based on biorhythm alignment
- **Mindset Intensity Scoring**: Measures how pronounced your current mindset is (0-100%)
- **Mindset Stability Analysis**: Determines how consistent your mental state is
- **Activity Optimization**: Ranks activities by compatibility with your current mindset
- **Mindset-Aware Decision Making**: Adjusts decision scores based on psychological state alignment
- **Transition Prediction**: Forecasts when you'll shift to a different mindset
- **Stress Detection**: Automatically identifies fractured/stressed states

### Advanced Features

- **Adaptive Learning**: Records decision outcomes and adjusts recommendations based on historical performance
- **Performance Analytics**: Tracks success rates, mindset frequencies, and optimal timing patterns
- **Critical Day Prediction**: Identifies high-risk days to avoid major decisions
- **Optimal Day Finding**: Predicts the best upcoming days for specific activities
- **Personalized Scheduling**: Generates optimized weekly schedules based on predicted mindsets
- **Day-of-Week Patterns**: Analyzes dominant mindsets by day of week
- **Historical Mindset Tracking**: Records and analyzes psychological state patterns over time
- **CSV Export**: Export decision and mindset history for external analysis
- **Comprehensive Analysis**: Full system view of biorhythms, mindsets, and decision readiness

## Architecture

### Biorhythm Engine (Original)

#### `BiorhythmCalculator`
Calculates biorhythm values using sinusoidal functions.

```csharp
var calculator = new BiorhythmCalculator(birthDate);
var profile = calculator.GetProfile(DateTime.Now);
// Returns: BiorhythmProfile with Physical, Emotional, Intellectual readings
```

#### `BiorhythmicDecisionEngine`
Provides decision recommendations based on biorhythmic alignment.

```csharp
var engine = new BiorhythmicDecisionEngine(birthDate);
var recommendation = engine.GetDecisionRecommendation("athletic", DateTime.Now);
// Returns: DecisionRecommendation with score and reasoning
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

### Mindset Alignment Engine (NEW)

#### `MindsetAlignmentAnalyzer`
Automatically detects psychological states from biorhythm patterns.

```csharp
var analyzer = new MindsetAlignmentAnalyzer(birthDate);
var mindset = analyzer.AnalyzeMindset(DateTime.Now);
// Returns: MindsetState with type, intensity, stability, characteristics,
//          strengths, weaknesses, recommended/avoid activities
```

#### `MindsetAlignedDecisionMaker`
Combines biorhythm and mindset analysis for intelligent recommendations.

```csharp
var maker = new MindsetAlignedDecisionMaker(birthDate);
var (recommendation, mindset) = maker.GetMindsetAlignedRecommendation("business", DateTime.Now);
// Score adjusted based on mindset alignment
// Includes mindset-specific insights
var activities = maker.GetMindsetOptimalActivities(DateTime.Now);
// Returns: Activities ranked by compatibility with current mindset
```

#### `MindsetTracker`
Tracks psychological patterns over time for personalization.

```csharp
var tracker = new MindsetTracker(birthDate);
tracker.RecordMindset(DateTime.Now);
var frequencies = tracker.GetMindsetFrequency(60);  // Last 60 days
var patterns = tracker.GetMindsetsByDayOfWeek();
var schedule = tracker.GetOptimizedScheduleRecommendation(startDate, 7);
```

### System Integration

```
┌─────────────────────────────────────────────────────┐
│         User Input (Decision/Activity)               │
└────────────────────┬────────────────────────────────┘
                     │
         ┌───────────┴───────────┐
         │                       │
    ┌────▼──────┐        ┌──────▼────┐
    │ Biorhythm │        │  Mindset  │
    │Calculator │        │ Analyzer  │
    └────┬──────┘        └──────┬────┘
         │                      │
         └──────────┬───────────┘
                    │
         ┌──────────▼──────────┐
         │  Decision Engine &  │
         │  Mindset Adjuster   │
         └──────────┬──────────┘
                    │
         ┌──────────▼──────────┐
         │ Recommendation &    │
         │ Mindset Insights    │
         └─────────────────────┘
```

### Data Flow

1. **Input**: User's birth date, target date, decision type
2. **Biorhythm Calculation**: Sine-wave values for all three cycles
3. **Mindset Detection**: Map biorhythm pattern to mindset type
4. **Intensity/Stability**: Calculate psychological state metrics
5. **Alignment Scoring**: Compare decision type to current mindset
6. **Recommendation**: Adjust base score by mindset alignment
7. **Output**: Score, recommendation, insights, activities, warnings

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

## Quick Start

### Basic Usage

```csharp
// Initialize the system
var birthDate = new DateTime(1990, 5, 15);

// Get today's mindset
var analyzer = new MindsetAlignmentAnalyzer(birthDate);
var mindset = analyzer.AnalyzeMindset(DateTime.Now);

Console.WriteLine($"Current Mindset: {mindset.Type}");
Console.WriteLine($"Intensity: {mindset.Intensity:P0}");
Console.WriteLine($"Stability: {mindset.Stability:P0}");

// Get mindset-aligned decision recommendation
var maker = new MindsetAlignedDecisionMaker(birthDate);
var (recommendation, state) = maker.GetMindsetAlignedRecommendation("business", DateTime.Now);
```

## Building and Running

### Prerequisites
- .NET 8.0 or higher
- C# 11.0 or higher

### Build

```bash
dotnet build
```

### Run Main Program

```bash
dotnet run --project BiorhythmDecisionSystem.csproj
```

### Run Mindset Integration Program

```bash
dotnet run MindsetIntegratedProgram.cs
```

### Interactive Menus

#### Main Program (BiorhythmDecisionEngine)
Traditional biorhythm-only features with full menu system:
1. Get decision recommendations
2. Adaptive recommendations from history
3. Record decision outcomes
4. View performance statistics
5. Find optimal days
6. Identify critical days
7. Detailed phase analysis
8. Comprehensive daily analysis

#### Mindset Program (MindsetIntegration)
Full system with automatic mindset alignment:
1. Today's mindset profile
2. 7-day mindset timeline
3. Activities optimized for current mindset
4. Mindset-aligned decision recommendations
5. Compare decision types by mindset
6. Record and track mindset patterns
7. Frequency analysis
8. Day-of-week pattern recognition
9. Optimized weekly scheduling
10. Complete system analysis combining biorhythms + mindsets

## Real-World Examples

### Example 1: Business Presentation Scheduling
```
Today:
  Mindset: Playful Spontaneous (Intensity: 78%, Stability: 65%)
  Decision: Schedule investor pitch
  Alignment: 52% (Poor for formal presentations)
  Recommendation: RESCHEDULE

Tomorrow:
  Mindset: Balanced Harmonious (Intensity: 91%, Stability: 92%)
  Decision: Schedule investor pitch
  Alignment: 98% (Excellent)
  Recommendation: SEIZE THIS WINDOW - Peak conditions for important presentation
```

### Example 2: Creative Writing
```
Detected Mindset: Creative Expressive
  - Intensity: 94% (peak creative state)
  - Stability: 88% (very consistent)
  - Characteristics: Imaginative, expressive, open to ideas
  - Duration: 4 more days
  - Recommendation: SEIZE OPPORTUNITY - Write novel chapter now
```

### Example 3: Difficult Negotiation
```
Current Mindset: Stressed Fractured
  - Intensity: 72% (pronounced stress)
  - Stability: 32% (very unstable)
  - Characteristics: Scattered, conflicted, overwhelmed
  - Best Activities: Self-care, breathing exercises, seeking support
  - Avoid: Major decisions, important meetings
  - Recommendation: DEFER negotiation by 3 days
  
In 3 Days:
  - Predicted: Emotional Intuitive (excellent for negotiation)
  - Alignment: 95%
```

## Limitations & Considerations

1. **Theory Status**: Biorhythm theory is pseudoscientific; use as a decision support tool, not the sole basis for important decisions
2. **Mindset Mapping**: Automatic mindset detection is pattern-based; individual psychology is more nuanced
3. **Accuracy**: Recommendations are probabilistic based on historical data and biorhythm theory
4. **Individual Variation**: Results may vary significantly between individuals
5. **External Factors**: Sleep, diet, stress, caffeine, and weather independently affect mindset
6. **Minimum History**: At least 3 recorded decisions recommended for adaptive learning
7. **Birth Date Accuracy**: Precise birth date required for accurate calculations
8. **Complementary Tool**: Best used alongside self-awareness, intuition, and professional advice

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

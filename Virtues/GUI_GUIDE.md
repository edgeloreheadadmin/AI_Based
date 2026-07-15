# The 13 Virtues System - HTML GUI Guide

## Overview

The HTML GUI is a fully interactive, self-contained web interface for tracking and developing virtue practice. Open `index.html` in any modern browser to start.

**No dependencies needed** — everything runs entirely in the browser with JavaScript.

## Features

### 🎯 Harmony Dashboard
The top section shows your overall virtue development:

- **Harmony Score**: Your current score (0-150+)
- **Progress Bar**: Visual representation of progress to mastery
- **Mastery Level**: Badge showing your current level
  - Novice (0-24)
  - Beginner (25-49)
  - Intermediate (50-74)
  - Advanced (75-99)
  - Mastery (100-149)
  - Exemplar (150+)

- **Quick Stats**:
  - Practice Days: Total days you've been practicing
  - Strongest Virtue: Your highest-scoring virtue
  - Active Virtues: Number of virtues you've practiced

### 💎 Virtue Cards (13 Total)

Each virtue has an interactive card showing:

- **Virtue Number** (1-13) in a colored circle
- **Name & Subtitle** (e.g., "Fortitude" - "The Flame Never Dies")
- **Description** of the virtue's essence
- **Your Score** prominently displayed
- **Quick Action Buttons**:
  - **+5 pts**: Small practice contribution
  - **+10 pts**: Moderate practice contribution

**Click any card** to open the detailed practice modal.

### 📖 Practice Modal

When you click a virtue card, a detailed modal opens with:

- **Full Philosophy**: The virtue's essence and meaning
- **Score Display**: Your current score for that virtue
- **4 Action Buttons** (specific to each virtue):
  - Each action adds 15 points
  - Examples: "Strengthen Foundation", "Sit in Silence", "Make Vows"
  
- **Custom Points Input**:
  - Enter any number of points
  - Add custom scores for personalized tracking

### 📊 Statistics Section

The bottom section shows aggregate data:

- **Total Points**: Sum of all virtue scores
- **Average Score**: Mean score across all virtues
- **Highest Scoring**: Maximum individual virtue score
- **Virtues Practiced**: Count of virtues with scores > 0

- **Reset Button**: Clear all progress and start fresh

## How to Use

### Daily Practice Flow

1. **Open the GUI**: Open `index.html` in your browser
2. **Choose a Virtue**: Click on a virtue card that calls to you
3. **Perform Actions**: Click action buttons or enter custom points
4. **Track Progress**: Watch your harmony score increase
5. **Repeat**: Practice different virtues throughout the day

### Example: Morning Routine

```
1. Click Fortitude → "Strengthen Foundation" → +15 pts
2. Click Patience → "Sit in Silence" → +15 pts
3. Click Grace → "Forgive" → +15 pts
4. Click Prayer → "Pray for Others" → +15 pts

Your Harmony Score increases from their collective growth
```

### Example: Working with One Virtue

1. Click **Diligence** card
2. Enter 30 points (representing an hour of focused work)
3. Click "Add Points"
4. See your score jump and harmony increase

## Scoring System

### How Points Work

- **+5 pts**: Small practice (e.g., remembered the principle)
- **+10 pts**: Moderate practice (e.g., spent time on it)
- **+15 pts**: Significant practice (e.g., performed an action)
- **Custom**: Any number you choose

### Harmony Score Calculation

Your overall Harmony Score is the **average** of all 13 virtue scores.

```
Harmony = (Sum of all virtue scores) / 13
```

This means:
- One very high virtue doesn't give you a high harmony score
- Balance across virtues matters
- Growth in "weak" virtues helps more than growth in "strong" ones

### Mastery Levels

```
Novice:       0-24    (Just starting)
Beginner:     25-49   (Building foundation)
Intermediate: 50-74   (Consistent practice)
Advanced:     75-99   (Deep understanding)
Mastery:      100-149 (Expert level)
Exemplar:     150+    (Beyond mastery)
```

## The 13 Virtues Reference

### 1. Fortitude 🔴
"The Flame Never Dies" — Face adversity, sit with pain, protect your core.

### 2. Chastity 🟠
Self-Discipline & Restraint — Establish laws, say no, channel desire.

### 3. Diligence 🟢
Time, Effort & Growth — Practice daily, build understanding, teach others.

### 4. Grace 🔵
Elegance & Flow — Move naturally, forgive deeply, reclaim your soul.

### 5. Honesty 🟣
Integrity & Truth — Face truth, acknowledge wrongs, see clearly.

### 6. Patience 🩷
Waiting & Understanding — Sit in silence, allow clarity, breathe.

### 7. Devotion 🟦
Integration of All — Align mental, physical, emotional, spiritual.

### 8. Prayer & Worship 🔴
Offering & Intention — Pray for others, clear mind, live as worship.

### 9. Divinity 🟣
Interconnectedness — See connections, protect next generation, be divine.

### 10. Constitution of Will 🟥
Inherited Strength — Light torches, choose love, become your will.

### 11. Deviation 🟦
Intelligent Evolution — Break rules wisely, adapt, evolve.

### 12. Sabbath & Rest 🔷
Balance & Restoration — Rest, restore energy, complete cycles.

### 13. Sacred Matrimony 🩹
Lifelong Commitment — Make vows, grow together, love continuously.

## Tips for Maximum Benefit

### 1. Practice Consistently
Don't try to max out all virtues at once. Pick 2-3 to focus on each week.

### 2. Interpret Scores Personally
The points are symbolic. What matters is the practice, not the number.

### 3. Use with the C# System
Track daily in this GUI, and run the C# system weekly for detailed analysis.

### 4. Combine Virtues
Notice how virtues support each other:
- Fortitude + Grace = Resilience
- Honesty + Patience = Wisdom
- Diligence + Devotion = Transformation

### 5. Reset When Needed
If you want to start fresh, use the Reset button. Consider resetting:
- Monthly for a new challenge
- Quarterly for seasonal focus
- Yearly for annual renewal

## Customization

### Saving Your Progress

**Note**: Progress is stored in browser memory and clears when you close the browser/clear cache.

**To Persist Data** (advanced users):
1. Right-click → Inspect
2. Open Console
3. Type: `localStorage.setItem('virtueScores', JSON.stringify(virtueScores))`

To restore:
```javascript
const saved = localStorage.getItem('virtueScores');
if (saved) Object.assign(virtueScores, JSON.parse(saved));
updateScores();
```

### Changing Colors

Edit the virtue definitions in the HTML:
```javascript
color: "#8b5cf6" // Change to your color code
```

### Adding Custom Actions

Add more action buttons by editing the virtue definitions:
```javascript
actions: [
    "Current Action",
    "New Custom Action"
]
```

## Keyboard Shortcuts

While using the GUI:

- **Esc**: Close modal
- **Click outside modal**: Close modal

## Browser Compatibility

Works on all modern browsers:
- ✅ Chrome/Edge 90+
- ✅ Firefox 88+
- ✅ Safari 14+
- ✅ Mobile browsers (responsive design)

## Dark Mode

The GUI automatically detects your system's theme preference:
- **Light Mode**: Clean, bright interface
- **Dark Mode**: Easy on the eyes, reduces eye strain

Toggle by changing your system theme settings.

## Performance

- **Instant loading**: No network requests needed
- **Smooth interactions**: All animations 60fps
- **Lightweight**: <50KB total size
- **No dependencies**: Pure HTML/CSS/JavaScript

## Troubleshooting

### Scores reset when I close the browser
This is normal — data is stored in browser memory. Use localStorage (see Customization) to persist.

### Modal won't close
Click outside the modal or press Esc. If it still won't close, refresh the page.

### Scores not updating
Try refreshing the page. If persists, check browser console for errors.

### Display looks broken
Ensure you're using a modern browser. Try clearing browser cache.

## Integration with C# System

### Workflow

1. **Daily**: Use HTML GUI to track quick practices
2. **Weekly**: Export your harmony score
3. **Monthly**: Run C# `VirtuesUnitTests` for deep analysis
4. **Quarterly**: Use `VirtueProgressionTracker` to assess growth

### Export Your Score

To export your current scores:

1. Right-click → Inspect → Console
2. Type: `console.log(virtueScores)`
3. Copy the output
4. Use it to seed the C# system

## Ideas for Using the GUI

### Morning Intention
Start each day by:
1. Opening the GUI
2. Selecting one virtue to focus on
3. Recording your intention

### Progress Check
End each day by:
1. Reviewing which virtues you practiced
2. Adding appropriate points
3. Noting your updated harmony score

### Weekly Reflection
Every Sunday:
1. Check your statistics
2. Identify your strongest virtue
3. Choose a weak virtue to focus next week
4. Optional: Reset for the new week

### Monthly Review
Every month:
1. Export your final scores
2. Reset if desired
3. Set new virtue goals
4. Celebrate progress

## Philosophy of the GUI

This interface is designed around the principle that:

> **Tracking makes tangible what is intangible.**

Virtue development is subtle and internal. This GUI makes your progress visible, encouragable, and measurable without reducing the profound spiritual reality to mere numbers.

The scores are symbols of commitment, not definitions of your virtue.

## Next Steps

1. **Open the GUI** and familiarize yourself
2. **Click a virtue** that resonates with you
3. **Perform an action** and watch your score grow
4. **Return tomorrow** and practice another virtue
5. **Build consistency** over days and weeks

## Support

For technical questions or feature requests, refer to:
- `IMPLEMENTATION_GUIDE.md` - Technical details
- `README.md` - Full system documentation
- `QUICK_REFERENCE.md` - Quick lookup

---

**"The Flame Never Dies. Through Virtue, We Become Divine."**

Begin today. Begin small. Begin now.

Your transformation through virtue starts with opening this page and clicking one button.

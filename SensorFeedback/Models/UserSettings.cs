using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tizen.System;

namespace SensorFeedback.Models
{
    class UserSettings
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        public bool ActivateVibrationFeedback { get; set; } = false;
        public bool ActivateSoundFeedback { get; set; } = false;
        public int VisualFeedbackType { get; set; } = 0;
    }
}

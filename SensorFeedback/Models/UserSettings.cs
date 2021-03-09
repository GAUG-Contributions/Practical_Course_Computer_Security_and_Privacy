using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tizen.System;
using static SensorFeedback.Services.RandomSensingService;

namespace SensorFeedback.Models
{
    class UserSettings
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        public bool ActivateVibrationFeedback { get; set; } = false;
        public bool ActivateSoundFeedback { get; set; } = false;
        public VisualFeedback VisualFeedbackType { get; set; } = VisualFeedback.Ring;
    }
}

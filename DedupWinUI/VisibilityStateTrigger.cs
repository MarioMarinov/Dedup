using Microsoft.UI.Xaml;

namespace DedupWinUI
{
    public class VisibilityStateTrigger : StateTriggerBase
    {
        private Visibility _visibility;
        private Visibility _triggerVisibility;

        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                _visibility = value;
                UpdateTrigger();
            }
        }

        public Visibility TriggerVisibility
        {
            get => _triggerVisibility;
            set
            {
                _triggerVisibility = value;
                UpdateTrigger();
            }
        }

        private void UpdateTrigger()
        {
            SetActive(Visibility == TriggerVisibility);
        }
    }
}


namespace SAIN.Classes
{
    public class DebugClass
    {
        public static string Reason(bool canSee)
        {
            string reason;
            if (canSee)
            {
                reason = "Can See";
            }
            else
            {
                reason = "Can't See";
            }
            return reason;
        }

        public static string Reason(StatusClass status)
        {
            string reason;
            if (status.Injured)
            {
                reason = "Injured";
            }
            else if (status.BadlyInjured)
            {
                reason = "Badly Injured";
            }
            else if (status.Dying)
            {
                reason = "Dying";
            }
            else
            {
                reason = "Healthy";
            }
            return reason;
        }
    }
}
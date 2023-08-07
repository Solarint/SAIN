using EFT;
using SAIN.SAINComponent.Classes.Info;

namespace SAIN.SAINComponent.BaseClasses
{
    public class AIInfoClass
    {
        public AIInfoClass(IAIDetails person)
        {
            Person = person;
            SAIN = BotOwner?.GetComponent<SAINComponentClass>();
        }

        public IAIDetails Person { get; private set; }
        public AiDataClass AIData => Person?.AIData;
        public BotOwner BotOwner => Person?.AIData?.BotOwner;
        public SAINComponentClass SAIN { get; private set; }
        public SAINBotInfoClass SAINBotInfo => SAIN?.Info;
        public SAINSquadClass SAINSquad => SAIN?.Squad;
    }
}
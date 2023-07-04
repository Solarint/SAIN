using UnityEngine;

namespace SAIN.Editor
{
    internal class RectLayout
    {

        public static int SelectedTab = 0;

        public const int TabCount = 6;

        public const int HomeTab = 0;
        public const int VisionTab = 1;
        public const int ShootTab = 2;
        public const int PersonalityTab = 3;
        public const int TalkTab = 4;
        public const int AdvancedTab = 5;
        public const int ExtractsTab = 6;
        public const int HearingTab = 7;

        public static Rect MainWindow = new Rect( 50 , 50 , 800f , 650f );
        public static Rect ExitButton => new Rect(770, 5, 25, 25);
        public static Rect DragRectangle => new Rect( 0, 0 , 770f , 30 );
        public static Rect ToolBarRectangle => new Rect( 10 , 30 , 780 , 60 );
        public static Rect ExpandableRectangle => new Rect(10, 10, 380, 380);
        public static Rect TabRectangle => new Rect( 10 , 100 , 780 , 540 );
        public static Rect EndBarRectangle => new Rect( 10 , 620 , 780 , 40 );
    }
}

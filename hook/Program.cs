using System;
using System.Windows.Forms;
using System.Diagnostics;
using static hook.hook;

namespace program
{
    class Incept
    {
        static Stopwatch watch;
        static Keys pressedKey;
        public static void Main()
        {
            watch = new Stopwatch();
            InterceptKeys.Hook();                               //Вешаем хук.
            InterceptKeys.KeyDown += InterceptKeys_KeyDown;     //событие "Клавиша нажата".
            InterceptKeys.KeyUp += InterceptKeys_KeyUp;         //событие "Клавиша отпущена".
            //InterceptKeys.KeyEvent += InterceptKeys_KeyEvent; //тоже можно юзать, но имхо такое
            //List<Keys> PressedKeys = InterceptKeys.KeysDown;    //Получаем список зажатых в настоящий момент клавиш.
            //InterceptKeys.KeyState IsCtrlPressed =
            //InterceptKeys.GetState(Keys.ControlKey);        //Получаем состояние клавиши CTRL

            Application.Run();
            InterceptKeys.UnHook();                             //Снимаем хук.
        }

        static void InterceptKeys_KeyEvent(Keys Key, InterceptKeys.KeyState State)
        {
            switch (State)
            {
                case InterceptKeys.KeyState.KeyDown:
                    watch.Start();
                    pressedKey = Key;
                    break;
                case InterceptKeys.KeyState.KeyUp:
                    watch.Stop();
                    Console.WriteLine("Time of " + Key + " key is: " + watch.Elapsed);
                    break;
            }
        }

        static void InterceptKeys_KeyUp(Keys Key)
        {
            watch.Stop();
            Console.WriteLine("Time of " + Key + " key is: " + watch.Elapsed);
        }

        static void InterceptKeys_KeyDown(Keys Key)
        {
            pressedKey = Key;
            watch.Start();
        }
    }
}
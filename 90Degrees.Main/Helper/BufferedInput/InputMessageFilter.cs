using System;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Twengine.Helper.BufferedInput
{
    public delegate void KeyboardHandler(KeyData keydata);

    public delegate void CharacterHandler(char c);

    [Flags]
    public enum KeyModifiers : int
    {
        None = 0x00,
        LeftControl = 0x01,
        RightControl = 0x02,
        Control = 0x03,
        LeftAlt = 0x04,
        RightAlt = 0x08,
        Alt = 0x0c,
        LeftShift = 0x10,
        RightShift = 0x20,
        Shift = 0x30,
    }

    public struct KeyData
    {
        public Keys Key;
        public KeyModifiers Modifier;
    }

    public class InputMessageFilter : System.Windows.Forms.IMessageFilter
    {
        public bool Enabled { get; set; }
        public bool TranslateMessage { get; set; }

        public Stack<KeyData> KeyData { get; private set; }

        public int Count
        {
            get { return KeyData.Count; }
        }

        public StringBuilder Text { get; private set; }

        public event KeyboardHandler KeyPressed;
        public event KeyboardHandler KeyReleased;
        public event KeyboardHandler KeyHeld;

        /// <summary>
        /// can contain '\n' = newline, '\b' = backspace
        /// </summary>
        public event CharacterHandler CharEntered;

        public InputMessageFilter()
        {
            KeyData = new Stack<KeyData>();
            Text = new StringBuilder();
            System.Windows.Forms.Application.AddMessageFilter(this);
        }

        public string GetText()
        {
            string text = Text.ToString();
            Text.Length = 0;
            return text;
        }

        #region IMessageFilter Members

        #region Nested

        protected enum Wm
        {
            Active = 6,
            Char = 0x102,
            KeyDown = 0x100,
            KeyUp = 0x101,
            SysKeyDown = 260,
            SysKeyUp = 0x105
        }

        protected enum Wa
        {
            Inactive,
            Active,
            ClickActive
        }

        protected enum Vk
        {
            Alt = 0x12,
            Control = 0x11,
            Shift = 0x10
        }

        #endregion

        #region Interop

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "TranslateMessage")]
        protected static extern bool _TranslateMessage(ref System.Windows.Forms.Message m);

        #endregion

        private KeyModifiers modifier;

        bool System.Windows.Forms.IMessageFilter.PreFilterMessage(ref System.Windows.Forms.Message m)
        {
            if (!Enabled)
                return false;
            //

            switch ((Wm) m.Msg)
            {
                case Wm.SysKeyDown:
                case Wm.KeyDown:
                    KeyData data;
                    bool held = false;
                    int bit30 = (m.LParam.ToInt32() & (1 << 30));
                    if (bit30 != 0) //previous state was pressed
                    {
                        held = true;
                    }
                    switch ((Vk) m.WParam)
                    {
                        case Vk.Control:
                            if ((m.LParam.ToInt32() & (1 << 24)) == 0)
                            {
                                data = new KeyData {Key = Keys.LeftControl, Modifier = modifier};
                                KeyData.Push(ref data);
                                modifier |= KeyModifiers.LeftControl;
                            }
                            else
                            {
                                data = new KeyData {Key = Keys.RightControl, Modifier = modifier};
                                KeyData.Push(ref data);
                                modifier |= KeyModifiers.RightControl;
                            }
                            break;
                        case Vk.Alt:
                            if ((m.LParam.ToInt32() & (1 << 24)) == 0)
                            {
                                data = new KeyData {Key = Keys.LeftAlt, Modifier = modifier};
                                KeyData.Push(ref data);
                                modifier |= KeyModifiers.LeftAlt;
                            }
                            else
                            {
                                data = new KeyData {Key = Keys.RightAlt, Modifier = modifier};
                                KeyData.Push(ref data);
                                modifier |= KeyModifiers.RightAlt;
                            }
                            break;
                        case Vk.Shift:
                            if ((m.LParam.ToInt32() & (1 << 24)) == 0)
                            {
                                data = new KeyData {Key = Keys.LeftShift, Modifier = modifier};
                                KeyData.Push(ref data);
                                modifier |= KeyModifiers.LeftShift;
                            }
                            else
                            {
                                data = new KeyData {Key = Keys.RightShift, Modifier = modifier};
                                KeyData.Push(ref data);
                                modifier |= KeyModifiers.RightShift;
                            }
                            break;
                            //
                        default:
                            data = new KeyData {Key = (Keys) m.WParam, Modifier = modifier};
                            KeyData.Push(ref data);
                            break;
                    }

                    //
                    if (TranslateMessage)
                        _TranslateMessage(ref m);
                    //
                    if (held)
                    {
                        FireKeyHeld(data);
                    }
                    else
                    {
                        FireKeyPress(data);
                    }
                    return true; //Hide from Forms

                case Wm.SysKeyUp:
                case Wm.KeyUp:
                    switch ((Vk) m.WParam)
                    {
                        case Vk.Control:
                            if ((m.LParam.ToInt32() & (1 << 24)) == 0)
                            {
                                modifier &= ~KeyModifiers.LeftControl;
                                data = new KeyData {Key = Keys.LeftControl, Modifier = modifier};
                            }
                            else
                            {
                                modifier &= ~KeyModifiers.RightControl;
                                data = new KeyData {Key = Keys.RightControl, Modifier = modifier};
                            }
                            break;
                        case Vk.Alt:
                            if ((m.LParam.ToInt32() & (1 << 24)) == 0)
                            {
                                modifier &= ~KeyModifiers.LeftAlt;
                                data = new KeyData {Key = Keys.LeftAlt, Modifier = modifier};
                            }
                            else
                            {
                                modifier &= ~KeyModifiers.RightAlt;
                                data = new KeyData {Key = Keys.RightAlt, Modifier = modifier};
                            }
                            break;
                        case Vk.Shift:
                            if ((m.LParam.ToInt32() & (1 << 24)) == 0)
                            {
                                modifier &= ~KeyModifiers.LeftShift;
                                data = new KeyData {Key = Keys.LeftShift, Modifier = modifier};
                            }
                            else
                            {
                                modifier &= ~KeyModifiers.RightShift;
                                data = new KeyData {Key = Keys.RightShift, Modifier = modifier};
                            }
                            break;
                        default:
                            data = new KeyData {Key = (Keys) m.WParam, Modifier = modifier};
                            break;
                    }
                    FireKeyRelease(data);

                    return true;

                case Wm.Char:
                    var c = (char) m.WParam;
                    if (c < (char) 0x20 && c != '\n' && c != '\r'
                        //&& c != '\t'//tab //uncomment to accept tab
                        && c != '\b') //backspace
                        break;

                    if (c == '\r')
                    {
                        c = '\n'; //Note: Control+ENTER will send \n, just ENTER will send \r
                    }

                    if (c == '\b' && Text.Length > 0)
                    {
                        Text.Remove(Text.Length - 1, 1); // don't append to textbuffer..
                        //Text.Length--;//pop 1
                    }
                    else
                    {
                        Text.Append(c);
                    }

                    FireCharReceived(c); // can contain '\n' = newline, '\b' = backspace

                    return true;

                case Wm.Active:
                    if (((int) m.WParam & 0xffff) == (int) Wa.Inactive)
                    {
                        modifier = KeyModifiers.None;
                    }
                    break; //Must not filter
            }
            return false;
        }

        private void FireCharReceived(char c)
        {
            if (CharEntered != null)
            {
                CharEntered(c);
            }
        }

        private void FireKeyPress(KeyData keydata)
        {
            if (KeyPressed != null)
            {
                KeyPressed(keydata);
            }
        }

        private void FireKeyHeld(KeyData keydata)
        {
            if (KeyHeld != null)
            {
                KeyHeld(keydata);
            }
        }

        private void FireKeyRelease(KeyData keydata)
        {
            if (KeyReleased != null)
            {
                KeyReleased(keydata);
            }
        }

        #endregion
    }
}
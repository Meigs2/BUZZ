using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BUZZ.Utilities;

namespace BUZZ.Core.Hotkeys
{
    /// <summary>
    /// Interaction logic for CharacterHotkeyWindow.xaml
    /// </summary>
    public partial class CharacterHotkeyWindow : Window
    {
        public List<Key> KeyList = new List<Key>();
        public List<ModifierKeys> ModifierKeyList = new List<ModifierKeys>();
        public bool Canceled = true;

        public CharacterHotkeyWindow()
        {
            InitializeComponent();
            KeyDown += CharacterHotkeyWindow_KeyDown;
        }

        private void CharacterHotkeyWindow_KeyDown(object sender, KeyEventArgs e)
        {
            KeyList.Clear();
            ModifierKeyList.Clear();
            var downKeys = KeyboardUtility.GetDownKeys().ToList();
            string s = string.Empty;
            if (downKeys.Count() > 1)
            {
                var firstKey = GetModifierKey(downKeys[0]);
                if (firstKey != ModifierKeys.None)
                {
                    s += firstKey;
                    ModifierKeyList.Add(firstKey);
                }
                else
                {
                    s += downKeys[0];
                    KeyList.Add(downKeys[0]);
                }

                // Convert to ModifierKeys and Keys
                for (int i = 1; i < downKeys.Count; i++)
                {
                    var key = downKeys[i];

                    var modifierKeys = GetModifierKey(key);
                    if (modifierKeys != ModifierKeys.None)
                    {
                        s += " + " + modifierKeys;
                        ModifierKeyList.Add(modifierKeys);
                    }
                    else
                    {
                        s += " + " + key;
                        KeyList.Add(key);
                    }
                }
            }
            else
            {
                foreach (var downKey in downKeys)
                {
                    var modifierKeys = GetModifierKey(downKey);
                    if (modifierKeys != ModifierKeys.None)
                    {
                        s += modifierKeys;
                        ModifierKeyList.Add(modifierKeys);
                    }
                    else
                    {
                        s += downKey;
                        KeyList.Add(downKey);
                    }
                }
            }
            TestingLabel.Content = s;
        }

        private static ModifierKeys GetModifierKey(Key key)
        {
            var modifierKeys = new ModifierKeys();
            if (key == Key.LeftCtrl || key == Key.RightCtrl)
            {
                modifierKeys = modifierKeys | ModifierKeys.Control;
            }
            if (key == Key.LWin || key == Key.RWin)
            {
                modifierKeys = modifierKeys | ModifierKeys.Windows;
            }
            if (key == Key.LeftShift || key == Key.RightShift)
            {
                modifierKeys = modifierKeys | ModifierKeys.Shift;
            }
            if (key == Key.LeftAlt || key == Key.RightAlt)
            {
                modifierKeys = modifierKeys | ModifierKeys.Alt;
            }

            return modifierKeys;
        }

        private void Button_Accept_Click(object sender, RoutedEventArgs e)
        {
            Canceled = false;
            this.Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

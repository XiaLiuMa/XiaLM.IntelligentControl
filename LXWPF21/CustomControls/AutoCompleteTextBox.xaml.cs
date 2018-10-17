using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CustomControls
{
    /// <summary>
    /// AutoCompleteTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class AutoCompleteTextBox : UserControl
    {
        private VisualCollection controls;
        private LxTextBox textBox;
        private ComboBox comboBox;
        private ObservableCollection<AutoCompleteEntry> autoCompletionList;
        private Timer keypressTimer;
        private delegate void TextChangedCallback();
        private bool insertText;
        private int delayTime;
        private int searchThreshold;

        public string Text
        {
            get { return textBox.Text; }
            set
            {
                insertText = true;
                textBox.Text = value;
            }
        }

        public int DelayTime
        {
            get { return delayTime; }
            set { delayTime = value; }
        }

        public int Threshold
        {
            get { return searchThreshold; }
            set { searchThreshold = value; }
        }

        public Style LxStyle
        {
            get { return textBox.Style; }
            set { textBox.Style = value; }
        }

        public AutoCompleteTextBox()
        {
            controls = new VisualCollection(this);
            InitializeComponent();

            autoCompletionList = new ObservableCollection<AutoCompleteEntry>();
            searchThreshold = 0;        // default threshold to 2 char
            delayTime = 100;
            keypressTimer = new System.Timers.Timer();  // set up the key press timer
            keypressTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            comboBox = new ComboBox();  // set up the text box and the combo box
            comboBox.Width = 300;
            comboBox.Height = 50;
            comboBox.Visibility = Visibility.Hidden;
            comboBox.IsSynchronizedWithCurrentItem = true;
            comboBox.IsTabStop = false;
            Panel.SetZIndex(comboBox, -1);
            comboBox.SelectionChanged += new SelectionChangedEventHandler(comboBox_SelectionChanged);
            textBox = new LxTextBox();
            textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);
            textBox.GotFocus += new RoutedEventHandler(textBox_GotFocus);
            textBox.KeyUp += new KeyEventHandler(textBox_KeyUp);
            textBox.KeyDown += new KeyEventHandler(textBox_KeyDown);
            textBox.VerticalContentAlignment = VerticalAlignment.Center;
            controls.Add(comboBox);
            controls.Add(textBox);
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            textBox.PreviewMouseDown += new MouseButtonEventHandler(TextBox_PreviewMouseDown);
        }

        private void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            textBox.Focus();
            e.Handled = true;
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != comboBox.SelectedItem)
            {
                insertText = true;
                ComboBoxItem cbItem = (ComboBoxItem)comboBox.SelectedItem;
                textBox.Text = cbItem.Content.ToString();
            }
        }

        /// <summary>
        /// 添加Item
        /// </summary>
        /// <param name="entry"></param>
        public void AddItem(AutoCompleteEntry entry)
        {
            autoCompletionList.Add(entry);
        }

        /// <summary>
        /// 清空Item
        /// </summary>
        /// <param name="entry"></param>
        public void ClearItem()
        {
            autoCompletionList.Clear();
        }

        private void TextChanged()
        {
            try
            {
                comboBox.Items.Clear();
                if (textBox.Text.Length >= searchThreshold)
                {
                    foreach (AutoCompleteEntry entry in autoCompletionList)
                    {
                        foreach (string word in entry.KeywordStrings)
                        {
                            if (word.Contains(textBox.Text))
                            {
                                ComboBoxItem cbItem = new ComboBoxItem();
                                cbItem.Content = entry.ToString();
                                comboBox.Items.Add(cbItem);
                                break;
                            }
                        }
                    }
                    comboBox.IsDropDownOpen = comboBox.HasItems;
                }
                else
                {
                    comboBox.IsDropDownOpen = false;
                }
            }
            catch { }
        }

        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            keypressTimer.Stop();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new TextChangedCallback(this.TextChanged));
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // text was not typed, do nothing and consume the flag
            if (insertText == true) insertText = false;

            // if the delay time is set, delay handling of text changed
            else
            {
                if (delayTime > 0)
                {
                    keypressTimer.Interval = delayTime;
                    keypressTimer.Start();
                }
                else TextChanged();
            }
        }

        //获得焦点时
        public void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // text was not typed, do nothing and consume the flag
            if (insertText == true) insertText = false;

            // if the delay time is set, delay handling of text changed
            else
            {
                if (delayTime > 0)
                {
                    keypressTimer.Interval = delayTime;
                    keypressTimer.Start();
                }
                else TextChanged();
            }
            //textBox.SelectAll();
            //textBox.PreviewMouseDown -= new MouseButtonEventHandler(TextBox_PreviewMouseDown);
        }

        public void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (textBox.IsInputMethodEnabled == true)
            {
                comboBox.IsDropDownOpen = false;
            }
        }

        /// <summary>
        /// 按向下按键时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && comboBox.IsDropDownOpen == true)
            {
                comboBox.Focus();
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            textBox.Arrange(new Rect(arrangeSize));
            comboBox.Arrange(new Rect(arrangeSize));
            return base.ArrangeOverride(arrangeSize);
        }

        protected override Visual GetVisualChild(int index)
        {
            return controls[index];
        }

        protected override int VisualChildrenCount
        {
            get { return controls.Count; }
        }
    }

    public class AutoCompleteEntry
    {
        private string[] keywordStrings;
        private string displayString;

        public string[] KeywordStrings
        {
            get
            {
                if (keywordStrings == null)
                {
                    keywordStrings = new string[] { displayString };
                }
                return keywordStrings;
            }
        }

        public string DisplayName
        {
            get { return displayString; }
            set { displayString = value; }
        }

        public AutoCompleteEntry(string name, params string[] keywords)
        {
            displayString = name;
            keywordStrings = keywords;
        }

        public override string ToString()
        {
            return displayString;
        }
    }

    public class LxTextBox : TextBox
    {
        public static DependencyProperty PlaceHolderProperty = DependencyProperty.Register("PlaceHolder", typeof(string), typeof(LxTextBox));

        /// <summary>
        /// 水印提示设置
        /// </summary>
        [Browsable(true)]
        public string PlaceHolder
        {
            get
            {
                return (string)GetValue(PlaceHolderProperty);
            }
            set
            {
                SetValue(PlaceHolderProperty, value);
            }
        }

        /// <summary>
        /// Type 输入类型
        /// 默认为0，不做任何限制
        /// 1：只能输入整形
        /// 2：只能输入整形和浮点型
        /// 3：可以输入字符以及数字
        /// </summary>
        [Browsable(true)]
        public int Type { get; set; } = 0;

        /// <summary>
        /// 设置为整形或者浮点型时最大值限制
        /// </summary>
        [Browsable(true)]
        public double MaxValue { get; set; } = 999999;

        /// <summary>
        /// 小数位数最大值
        /// </summary>
        [Browsable(true)]
        public uint MaxDecimalPlaces { get; set; } = 4;

        /// <summary>
        /// 设置为整形或浮点型的最小值设置   注意：只对负数最小值做限制
        /// </summary>
        [Browsable(true)]
        public double MinValue { get; set; } = -999999;

        /// <summary>
        /// 设置属性，如果为true表示不可为空，如为空会自动给默认值0
        /// </summary>
        [Browsable(true)]
        public bool IsEmpty { get; set; } = false;


        static LxTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LxTextBox), new FrameworkPropertyMetadata(typeof(LxTextBox)));
        }

        public LxTextBox()
        {
            DataObject.AddPastingHandler(this, PasteHandler);
            this.PreviewMouseDown += new MouseButtonEventHandler(LD_TextBox_PreviewMouseDown);
            this.GotFocus += new RoutedEventHandler(LD_TextBox_GotFocus);
            this.LostFocus += new RoutedEventHandler(LD_TextBox_LostFocus);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (this.Type != 0) //当Type不等于0时做输入法限制设置
            {
                InputMethod.SetIsInputMethodEnabled(this, false);
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (this.Text == string.Empty)
            {
                VisualStateManager.GoToState(this, "Empty", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "NotEmpty", true);
            }
            base.OnTextChanged(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (this.Type != 0)
            {
                if (e.Key == Key.Space)
                {
                    e.Handled = true;
                }
                if (this.SelectedText.Length <= this.Text.Length && e.Key != Key.RightShift && e.Key != Key.LeftShift && e.Key != Key.RightCtrl && e.Key != Key.LeftCtrl && e.Key != Key.Left && e.Key != Key.Right && e.Key != Key.C)
                {
                    this.SelectedText = "";
                }
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            string type1 = "0123456789-";
            string type2 = "0123456789-.";
            string type3 = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            //最大值与最小值限制设置
            string afterInputText = Text.Insert(this.SelectionStart, e.Text);
            if (Type == 1)
            {
                int.TryParse(afterInputText, out int result);
                if (!type1.Contains(e.Text))
                {
                    e.Handled = true;
                }
                if (this.Text == "0" && e.Text == "0")
                {
                    e.Handled = true;
                }
                if (e.Text == "-" && (this.Text.IndexOf("-") > -1 || afterInputText.IndexOf("-") != 0))
                {
                    e.Handled = true;
                }
                if (result > MaxValue)
                {
                    e.Handled = true;
                }
                if (MinValue < 0)
                {
                    if (result < 0 && Math.Abs(result) > Math.Abs(MinValue))
                    {
                        e.Handled = true;
                    }
                }

            }
            else if (this.Type == 2)
            {
                double.TryParse(afterInputText, out double result);
                if (!type2.Contains(e.Text))
                {
                    e.Handled = true;
                }
                if (this.SelectedText.Length <= this.Text.Length)
                {
                    if (this.Text == "" && e.Text == ".")
                    {
                        e.Handled = true;
                    }
                    if ((this.Text == "0" || this.Text == "-0") && e.Text != ".")
                    {
                        e.Handled = true;
                    }
                    if (this.Text.IndexOf("0") == 0 && e.Text == "0")
                    {
                        e.Handled = true;
                    }
                    if (this.Text.IndexOf(".") > 0)
                    {
                        string[] str = afterInputText.Split('.');
                        if (str[1].Length > MaxDecimalPlaces)
                        {
                            e.Handled = true;
                        }
                    }
                    if (this.Text.IndexOf(".") > 0 && e.Text == ".")
                    {
                        e.Handled = true;
                    }
                    if (e.Text == "-" && (this.Text.IndexOf("-") > -1 || afterInputText.IndexOf("-") != 0))
                    {
                        e.Handled = true;
                    }
                    if (this.Text == "-" && e.Text == ".")
                    {
                        e.Handled = true;
                    }
                    if (result > MaxValue)
                    {
                        e.Handled = true;
                    }
                    if (MinValue < 0)
                    {
                        if (result < 0 && Math.Abs(result) > Math.Abs(MinValue))
                        {
                            e.Handled = true;
                        }
                    }
                }
            }
            else if (Type == 3)
            {
                if (!type3.Contains(e.Text))
                {
                    e.Handled = true;
                }
            }

            base.OnPreviewTextInput(e);
        }

        void PasteHandler(object sender, DataObjectPastingEventArgs e)
        {
            //最大值与最小值粘贴限制
            if (Type == 1)
            {
                if (!e.DataObject.GetDataPresent(typeof(String)))
                {
                    e.CancelCommand();
                    return;
                }
                String text = (String)e.DataObject.GetData(typeof(String));
                int temp = 0;
                if (MaxValue != 0)
                {
                    text += this.Text;
                    if (!int.TryParse(text, out temp) || temp > MaxValue)
                    {
                        e.CancelCommand();
                    }
                }
                else
                {
                    if (!int.TryParse(text, out temp))
                    {
                        e.CancelCommand();
                    }
                }
                if (MinValue < 0)
                {
                    text += this.Text;
                    if (!int.TryParse(text, out temp) || Math.Abs(temp) > Math.Abs(MinValue))
                    {
                        e.CancelCommand();
                    }
                }
                else
                {
                    if (!int.TryParse(text, out temp))
                    {
                        e.CancelCommand();
                    }
                }
            }
            else if (Type == 2)
            {
                if (!e.DataObject.GetDataPresent(typeof(String)))
                {
                    e.CancelCommand();
                    return;
                }
                String text = (String)e.DataObject.GetData(typeof(String));
                text += this.Text;
                double temp = 0;
                if (MaxValue != 0)
                {
                    if (!double.TryParse(text, out temp) || temp > MaxValue)
                    {
                        e.CancelCommand();
                    }
                }
                else
                {
                    if (!double.TryParse(text, out temp))
                    {
                        e.CancelCommand();
                    }
                }
                if (MinValue < 0)
                {
                    text += this.Text;
                    if (!double.TryParse(text, out temp) || Math.Abs(temp) > Math.Abs(MinValue))
                    {
                        e.CancelCommand();
                    }
                }
                else
                {
                    if (!double.TryParse(text, out temp))
                    {
                        e.CancelCommand();
                    }
                }
            }
        }

        void LD_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.PreviewMouseDown += new MouseButtonEventHandler(LD_TextBox_PreviewMouseDown);
        }

        void LD_TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
            e.Handled = true;
        }

        void LD_TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.SelectAll();
            this.PreviewMouseDown -= new MouseButtonEventHandler(LD_TextBox_PreviewMouseDown);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            int result;
            int.TryParse(this.Text, out result);
            if (!IsEmpty && (Type == 1 || Type == 2))
            {
                if (this.Text == string.Empty)
                {
                    this.Text = "0";
                }
            }
            if (result < MinValue)
            {
                this.Text = MinValue.ToString();
            }
            base.OnLostFocus(e);
        }

    }
}

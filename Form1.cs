using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace WindowsFormsApp1
{
    public unsafe partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            serialPort1.Encoding = Encoding.GetEncoding("GB2312");
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;

        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        ////搜索串口部分
        private void button1_Click(object sender, EventArgs e)
        {
            SearchAnAddSerialToComboBox(serialPort1, comboBox1);
        }
        //搜索串口函数————将可用的串口号添加到ComboBox
        private void SearchAnAddSerialToComboBox(SerialPort MyPort, ComboBox MyBox)
        {
            string[] NumberOfPort = new string[20];         //最多容纳20个，否则影响运行效率
            string MidString1;                              //中间数组，用于缓存
            MyBox.Items.Clear();                            //清空ComboBox的内容
            for (int i = 1; i < 20; i++)
            {
                try                                         //核心是靠try-catch完成遍历     
                {
                    MidString1 = "COM" + i.ToString();      //将串口名赋给MidString1
                    MyPort.PortName = MidString1;           //把MidString1赋给MyPort.PortName
                    MyPort.Open();
                    NumberOfPort[i - 1] = MidString1;       //依次将MidString1的字符赋给NumberOfPort
                    MyBox.Items.Add(MidString1);            //打开成功，添加到列表
                    MyPort.Close();
                    MyBox.Text = NumberOfPort[i - 1];       //显示最后扫描成功的串口
                }
                catch { }
            }
        }

        ////打开串口部分
        private void button2_Click(object sender, EventArgs e)
        {
            try {
                //根据当前串口的属性判断是否打开
                if (serialPort1.IsOpen)      //串口已打开
                {
                    serialPort1.Close();     //将串口关掉
                    button2.Text = "打开串口";
                    button2.BackColor = Color.ForestGreen;
                    label6.Text = "串口已关闭";
                    label6.ForeColor = Color.Red;
                    button4.Enabled = false;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                    textBox1.Text = "";     //清空接收区
                    textBox2.Text = "";     //清空发送区
                }
                else        //串口出于关闭状态，则设置好串口属性后再打开
                {
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                    serialPort1.DataBits = Convert.ToInt16(comboBox3.Text);

                    if (comboBox4.Text.Equals("None"))
                        serialPort1.Parity = System.IO.Ports.Parity.None;
                    else if (comboBox4.Text.Equals("Odd"))
                        serialPort1.Parity = System.IO.Ports.Parity.Odd;
                    else if (comboBox4.Text.Equals("Even"))
                        serialPort1.Parity = System.IO.Ports.Parity.Even;
                    else if (comboBox4.Text.Equals("Mark"))
                        serialPort1.Parity = System.IO.Ports.Parity.Mark;
                    else if (comboBox4.Text.Equals("Space"))
                        serialPort1.Parity = System.IO.Ports.Parity.Space;

                    if (comboBox5.Text.Equals("1"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.One;
                    else if (comboBox5.Text.Equals("1.5"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.OnePointFive;
                    else if (comboBox5.Text.Equals("2"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.Two;

                    serialPort1.Open();         //设置完毕，打开串口
                    button2.Text = "关闭串口";
                    button2.BackColor = Color.Firebrick;
                    label6.Text = "串口已打开";
                    label6.ForeColor = Color.Green;
                    button4.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                //捕获可能发生的异常并处理
                //捕获到异常， 创建一个新的对象，之前的不可以再次使用
                serialPort1 = new System.IO.Ports.SerialPort();
                //刷新COM口选项
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                //响铃将异常反馈给用户
                System.Media.SystemSounds.Beep.Play();
                button2.Text = "打开串口";
                button2.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;

            }

        }

        ////清空输出部分
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";         //清空接收框
            textBox2.Text = "";         //清空发送框

            send_count = 0;             //发送计数清零
            received_count = 0;         //接收计数清零
            label7.Text = "Tx:" + send_count.ToString() + " Bytes";      //刷新界面
            label8.Text = "Rx:" + received_count.ToString() + " Bytes";  //刷新界面
        }

        ////数据发送部分
        private long send_count = 0;
        private void button4_Click(object sender, EventArgs e)
        {
            byte[] temp = new byte[1];
            try
            {
                //首先判断串口是否开启
                if (serialPort1.IsOpen)
                {
                    int num = 0;        //获取本次发送的字节数
                    //串口出于开启状态，将发送区文本发送

                    //判断发送模式
                    if (radioButton4.Checked)
                    {
                        //HEX:
                        //需要用正则表达式将用户输入字符中的十六进制字符匹配出来
                        string buf = textBox2.Text;
                        string pattern = @"\s";
                        string replacement = "";
                        Regex rgx = new Regex(pattern);
                        string send_data = rgx.Replace(buf, replacement);

                        //不发送新行
                        num = (send_data.Length - send_data.Length % 2) / 2;
                        for (int i = 0; i < num; i++)
                        {
                            temp[0] = Convert.ToByte(send_data.Substring(i * 2, 2), 16);
                            serialPort1.Write(temp, 0, 1);      //循环发送
                        }
                        //如果用户输入的字符为奇数，则单独处理
                        if (send_data.Length % 2 != 0)
                        {
                            temp[0] = Convert.ToByte(send_data.Substring(textBox2.Text.Length - 1, 1), 16);
                            serialPort1.Write(temp, 0, 1);
                            num++;
                        }

                        //判断是否自动发送新行
                        if (checkBox2.Checked)
                        {
                            //自动发送新行
                            serialPort1.WriteLine("");
                        }
                    }
                    else
                    {
                        //ASCII:
                        //判断是否自动发送新行
                        if (checkBox2.Checked)
                        {
                            //自动发送新行
                            serialPort1.WriteLine(textBox2.Text);
                            num = textBox2.Text.Length + 2;         //回车占两个字节
                        }
                        else
                        {
                            //不自动发送新行
                            serialPort1.Write(textBox2.Text);
                            num = textBox2.Text.Length;
                        }
                    }

                    send_count += num;
                    label7.Text = "Tx:" + send_count.ToString() + " Bytes";
                }
            }
            catch (Exception ex)
            {
                //捕获到异常，创建一个新的对象，之前的不可再用
                serialPort1 = new System.IO.Ports.SerialPort();
                //刷新COM口选项
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                //响铃并显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                button2.Text = "打开串口";
                button2.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;

            }
        }

        ////数据接收部分
        private long received_count = 0;
        private StringBuilder sb = new StringBuilder();
        private DateTime current_time = new DateTime();
        //private int data_length = 0;
        //private byte[] received_data = new byte[13];//此处定义接收数据长度


        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int num = serialPort1.BytesToRead;      //获取接收缓冲区中的字节数
            byte[] received_buf = new byte[num];    //声明一个大小为num的字节数据用于存放读出的byte型数据

            received_count += num;                  //接收字节计数变量增加num
            serialPort1.Read(received_buf, 0, num); //读取接收缓存区中num个字节到byte数组中

            sb.Clear();                             //防止出错，先清空字符串构造器

            if (radioButton2.Checked)
            {
                //HEX：
                foreach (byte b in received_buf)         //遍历数组进行字符串转化及拼接
                {
                    sb.Append(b.ToString("X2") + ' ');
                }
            }
            else
            {
                //ASCII：
                sb.Append(Encoding.ASCII.GetString(received_buf)); //将整个数组解码为ASCII数组
            }

            try
            {
                //因为要访问UI资源，所以要使用invoke方式同步UI
                Invoke((EventHandler)(delegate
                {
                    if (checkBox1.Checked)
                    {
                        //显示时间
                        current_time = System.DateTime.Now;         //获取当前时间
                        textBox1.AppendText(current_time.ToString("HH:mm:ss") + "  " + sb.ToString());
                    }
                    else
                    {
                        //不显示时间
                        textBox1.AppendText(sb.ToString());
                    }
                    //textBox3.Text = received_buf.Length.ToString();
                    label8.Text = "Rx:" + received_count.ToString() + " Bytes";

                }
                    )
                );
            }
            catch (Exception ex)
            {
                //响铃并显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show(ex.Message);
            }

            //利用for循环组合，将接收的数组存储在received_data中，从而进行判断
            //for (int i = 0; i < received_buf.Length; i++)
            //{
            //    received_data[data_length] = received_buf[i];
            //    data_length++;
            //}
            //下面可继续写判断语句，并且不要忘记在执行判断之后归零判断依据

        }

        ////自动发送部分
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                //自动发送选中，开始自动发送
                numericUpDown1.Enabled = false;         //不可调节时间
                timer1.Interval = (int)numericUpDown1.Value;
                timer1.Start();
                label6.Text = "串口已打开，自动发送中………";
                label6.ForeColor = Color.Green;
            }
            else
            {
                //自动发送功能未选中，不自动发送
                numericUpDown1.Enabled = true;
                timer1.Stop();
                label6.Text = "串口已打开";
                label6.ForeColor = Color.Green;
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //定时时间到
            button4_Click(button4, new EventArgs());
        }

        #region/*fw代码*/
        //此处代码删掉就会报错，但本身没有任何影响
        private void label9_Click(object sender, EventArgs e)
        {

        }
        #endregion    
    }
        
}

using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace Program_process_monitoring
{
    public partial class ProcessMonitoring : Form
    {
        public ProcessMonitoring()
        {
            InitializeComponent();

        }

        private int c = 0;
        private bool f = false;
        private int Proc_Id = 0;
        private string infoWS = null;
        private string infoHC = null;
        private string infoCPU = null;
        private string infoPMS = null;
        private string infoTime = null;
        private string Proc_Dir = null;
        private string Proc_Name = null;

        private string GetProcessName() //Метод получения имени процесса из пути до исполняемого файла
        {
            string PrN;
            string Proc_Path = GetProcessDir();
            string[] subs = Proc_Path.Split('\\', '.');
            PrN = subs[subs.Length - 2];
            return PrN;
        }

        private string GetProcessDir() => myProcess.StartInfo.FileName; //Метод получения пути до исполняемого файла

        private int GetProcessId(string PrN) //Метод получения ID запущенного процесса
        {
            Process[] processlist = Process.GetProcesses();
            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName == PrN)
                    Proc_Id = theprocess.Id;
            }
            return Proc_Id;
        }


        private bool ProcessStart(string PrN) //Метод проверки на наличие запущенного процесса
        {
            Process[] processes = Process.GetProcessesByName(PrN);
            return processes.Length != 0 ? true : false;
        }

        private void Info_Save() //Метод сохранения полученной информации
        {
            string infoAll = null;

            infoAll = "Название процесса\n" + Proc_Name +
                        "\n\nПуть до исполняемого файла\n" + Proc_Dir +
                        "\n\nВремя в секундах\n" + infoTime +
                        "\n\nНагрузка на процессор в процентах\n" + infoCPU +
                        "\n\nОбъём физической памяти, испульзуемой процесом (Мб)\n" + infoWS +
                        "\n\nОбъём виртаульной памяти, зарезервированной ОС для процесса (Мб)\n" + infoPMS +
                        "\n\nЧисло дескрипторов операционной системы, открытых процессом\n" + infoHC;

            Proc_Id = 0;
            infoWS = null;
            infoHC = null;
            infoCPU = null;
            infoPMS = null;
            infoTime = null;

            saveFileDialog1.FileName = Proc_Name;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                File.WriteAllText(saveFileDialog1.FileName, infoAll);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                using (myProcess = Process.GetProcessById(Proc_Id)) //Присоединение к запущенному процессу через его ID
                {
                    infoTime += Convert.ToString(c += Convert.ToInt32(textBox2.Text)) + "\t";
                    myProcess.Refresh();
                    performanceCPU.NextValue(); //Получение значения нагрузки на CPU
                    System.Threading.Thread.Sleep(500);
                    infoCPU += Convert.ToString(Math.Round(performanceCPU.NextValue(), 1)) + "\t"; //Получение актуального значения нагрузки на CPU
                    infoWS += Convert.ToString(myProcess.WorkingSet64 / 1000000) + "\t"; //Получение значения используемой физической памяти
                    infoPMS += Convert.ToString(myProcess.PrivateMemorySize64 / 1000000) + "\t"; //Получение зарезервированной виртуальной памяти
                    infoHC += Convert.ToString(myProcess.HandleCount) + "\t"; //Получение актуального значения дескрипторов
                    f = true;

                    label1.Text = "Время: " + Convert.ToString(c) + "с";
                }
            }
            catch
            {
                myProcess.Dispose();
                timer1.Stop();

                if (f)
                {
                    Info_Save();
                }
                else
                {
                    MessageBox.Show("Вы быстро закрыли процесс, программа не успела получить все данные!");
                }
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Proc_Dir = openFileDialog1.FileName;
                textBox1.Text = Proc_Dir;
                label1.Text = "Время: 0с";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;

            timer1.Interval = Convert.ToInt32(textBox2.Text) * 1000;
            c = 0;

            myProcess = new Process();
            myProcess.StartInfo.FileName = Proc_Dir;

            if (!File.Exists(Proc_Dir)) //Проверка на наличие файла
            {
                MessageBox.Show("Вы не выбрали путь до исполняемого файла!");
                button1.Enabled = true;
                button2.Enabled = true;
                return;
            }

            Proc_Name = GetProcessName();
            try
            {
                if (ProcessStart(Proc_Name))
                {
                    Proc_Id = GetProcessId(Proc_Name);
                    timer1.Start();
                }
                else
                {
                    myProcess.Start();
                    Proc_Id = myProcess.Id;
                    timer1.Start();
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("Вы запустили приложение с повышенными правами и не " +
                    "предоставили разрешение на внесение изменений на устройстве!");
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }
    }
}
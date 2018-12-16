using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace I_like_trains
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class graph
        {
            public int nvertices;
            public int[] m;
            public int[] visited;
        }
        public static void graph_clear(graph g)
        {
            int i;
            int j;
            for (i = 0; i < g.nvertices; i++)
            {
                g.visited[i] = 0;
                for (j = 0; j < g.nvertices; j++)
                {
                    g.m[i * g.nvertices + j] = 0;
                }
            }
        }
        public static graph graph_create(int nvertices)
        {
            graph g;
            g = new graph();
            g.nvertices = nvertices;
            g.visited = new int[nvertices];
            g.m = new int[nvertices * nvertices];
            graph_clear(g);
            return g;
        }

        
        public static void graph_set_edge(graph g, int i, int j, int w)
        {
            g.m[(i - 1) * g.nvertices + j - 1] = w;
            g.m[(j - 1) * g.nvertices + i - 1] = w;
        }
        public static int graph_get_edge(graph g, int i, int j)
        {
            return g.m[(i - 1) * g.nvertices + j - 1];

        }


        private void NeFloydWarshall(graph g, int v, int i1, int i2)
        {
            int[] w = new int[v * v];
            for (int i = 0; i < v; i++)
            {
                for (int u = 0; u < v; u++)
                {
                    w[i * v + u] = g.m[i * v + u];
                }
            }
            for (int i = 0; i < v; i++)
            {
                for (int u = 0; u < v; u++)
                {
                    for (int t = 0; t < v; t++)
                    {
                        if (w[u * v + i] > 0 && w[i * v + t] > 0)
                        {
                            if (w[u * v + t] == 0)
                            {
                                w[u * v + t] = w[u * v + i] + w[i * v + t];
                            }
                            else
                            {
                                w[u * v + t] = Math.Min(w[u * v + t], w[u * v + i] + w[i * v + t]);
                            }
                       }
                    }
                }
            }
           if (i1 != i2)
            {
                label1.Text = "Со станции " + Convert.ToString(allItems[i1])+" до станции ";
                label4.Text = Convert.ToString(allItems[i2]) + " можно добраться таким"; 
                label5.Text = " кол-вом поездов:" + Convert.ToString(w[i1 * v + i2]);
            }
           else label1.Text = "Вы и так на станции "+Convert.ToString(allItems[i1]);
        }
        static string[] SearchDirections(string[] body, string url)// загружаем информацию с сайта о всех электричках с данной станции
        {
            string HtmlText = string.Empty;
            HttpWebRequest myHttwebrequest = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse myHttpWebresponse = (HttpWebResponse)myHttwebrequest.GetResponse();
            StreamReader strm = new StreamReader(myHttpWebresponse.GetResponseStream());
            HtmlText = strm.ReadToEnd();// код нашей html страницы
            MatchCollection matches = Regex.Matches(HtmlText, @"class=""train_direction"">(.*?)<", RegexOptions.Singleline);// получаем температуру с сайта
            int n = 0;
            foreach (Match match in matches)
            {
                Array.Resize(ref body, body.Length + 1);
                body[n] = match.Groups[1].Value;
                n++;
            }
            return body;  
        }
        static string[] AllStations(string[] body, string[] allItems)// получаем список всех станций, которые есть в расписании
        {
            int n = 0;
            for (int i = 0; i < body.Length; i++)
            {
                string[] helpString = body[i].Split(new[] { " → " }, StringSplitOptions.RemoveEmptyEntries);
                Array.Resize(ref allItems, allItems.Length + 2);
                allItems[n] = helpString[0]; n++;
                allItems[n] = helpString[1]; n++;
            }
            return allItems; // резульат
        }
        static string[] Unicum(string[] allItems)// удаляем вс повторяющиеся значения из массива
        {
            string[] result = allItems.Distinct().ToArray();
            return result; 
        }
        public void TakeUnicum(ref string[] body,ref string[] allItems, string url)//получаем список уникальных станций
        {            
            body = SearchDirections(body,  url);
            allItems =Unicum(AllStations(body, allItems));
        }
      static void AddToGraph(string[] body, string[] allItems, string start,ref graph g)// добавляем в граф все наши станции
        {
            for (int i = 0; i < body.Length; i++)
            {
               string[] helpString = body[i].Split(new[] { " → " }, StringSplitOptions.RemoveEmptyEntries);
                graph_set_edge(g, Array.IndexOf(allItems, start)+1, Array.IndexOf(allItems, helpString[0])+1, 1);
                graph_set_edge(g, Array.IndexOf(allItems, start)+1, Array.IndexOf(allItems, helpString[1])+1, 1);
            }
        }
    
              private bool CheckConnection()//Проверка подключения к интернету
                {
                    WebClient client = new WebClient();
                    try
                    {
                        using (client.OpenRead("http://www.google.com"))
                        {
                        }
                        return true;
                    }
                    catch (WebException)
                    {
                        return false;
                    }
                }
        private void button1_Click(object sender, EventArgs e)
        {
            label4.Text = "";
            label5.Text = "";
            string t1 = Convert.ToString(textBox1.Text);
            string t2 = Convert.ToString(textBox2.Text);
            if (Array.IndexOf(allItems, t1) != -1 && Array.IndexOf(allItems, t2) != -1)
            {
               NeFloydWarshall(g, allItems.Length, Array.IndexOf(allItems, t1), Array.IndexOf(allItems, t2));
            }
            else { label1.Text = "Некоректно введено название одного";
            label4.Text = " из городов.";}
         }
        
        string[] allItems;
        graph g;
        private void Form1_Load(object sender, EventArgs e)
        {
            if (CheckConnection())
            {
                listBox2.Items.Clear();
                string[] bodyDP = new string[0];
                string[] allItemsDP = new string[0];
                string urlDP = @"http://dnepropetrovsk.elektrichki.net/raspisanie/dnepropetrovsk-glav/";
                string[] bodyZP = new string[0];
                string[] allItemsZP = new string[0];
                string urlZP = @"http://zaporozhe.elektrichki.net/raspisanie/zaporozhe-1/";
                string[] bodyKR = new string[0];
                string[] allItemsKR = new string[0];
                string urlKR = @"http://dnepropetrovsk.elektrichki.net/raspisanie/krivoj-rog-glavnyj/";
                string[] bodyKH = new string[0];
                string[] allItemsKH = new string[0];
                string urlKH = @"http://harkov.elektrichki.net/raspisanie/harkov-pass/";
                TakeUnicum(ref bodyDP, ref allItemsDP, urlDP);
                TakeUnicum(ref bodyZP, ref allItemsZP, urlZP);
                TakeUnicum(ref bodyKR, ref allItemsKR, urlKR);
                TakeUnicum(ref bodyKH, ref allItemsKH, urlKH);


                allItems = new string[allItemsDP.Length + allItemsZP.Length + allItemsKR.Length + allItemsKH.Length];
                allItemsDP.CopyTo(allItems, 0);
                allItemsZP.CopyTo(allItems, allItemsDP.Length);
                allItemsKR.CopyTo(allItems, allItemsDP.Length + allItemsZP.Length);
                allItemsKH.CopyTo(allItems, allItemsDP.Length + allItemsZP.Length + allItemsKR.Length);
                allItems = Unicum(allItems);
                for (int i = 0; i < allItems.Length; i++)
                { listBox2.Items.Add(allItems[i]); }
                g = graph_create(allItems.Length);
                AddToGraph(bodyDP, allItems, "Днепропетровск", ref g);
                AddToGraph(bodyZP, allItems, "Запорожье", ref g);
                AddToGraph(bodyKR, allItems, "Кривой Рог", ref g);
                AddToGraph(bodyKH, allItems, "Харьков", ref g);
            }
            else { label1.Text = "Отсутствует подключение к интернету";
            button1.Visible = false;
            }
        }
    }
}

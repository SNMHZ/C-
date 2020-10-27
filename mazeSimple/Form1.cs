using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mazeSimple
{
    public partial class Form1 : Form
    {
        int w_num, h_num ;
        int[,] map;
        Point start_p, end_p;
        int current_x, current_y;
        int side_len;
        bool mouse_down_flag, edit_flag, setStart_flag, setEnd_flag;
        Form2 frm2;
        static List<Tile> path;
        int path_len, timer;

        public Form1()
        {
            InitializeComponent();
            w_num = h_num = 20;
            map = new int[w_num, h_num];
            start_p.X = start_p.Y = 0;
            end_p.X = w_num-1;
            end_p.Y = h_num-1;
            setEnd_flag =setStart_flag = edit_flag = mouse_down_flag = false;
            path_len = -1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen skyBluePen = new Pen(Brushes.DeepSkyBlue);
            SolidBrush skyBluePenBrush = new SolidBrush(Color.Blue);
            int padding = 20;
            
            //가로변 길이와 세로변 길이가 같다.
            // ->가로와 세로 클라이언트 사이즈 비교 후 더 짧은거 채택해서 나눠주기.
            if (ClientSize.Width < ClientSize.Height)
            {
                side_len = (ClientSize.Width-padding*2) / 20;
            }
            else
            {
                side_len = (ClientSize.Height-padding*2) / 20;
            }



            //지도 그리기
            if (path_len != -1)
            {
                for(int i=0; i<timer; i++)
                {
                    SolidBrush pathBrush = new SolidBrush(Color.Gold);
                    g.FillRectangle(pathBrush, new Rectangle(padding + path[i].X * side_len, padding + path[i].Y * side_len, side_len, side_len));
                }
            }

            for (int i = 0; i < w_num; i++)
                for (int j = 0; j < h_num; j++)
                {
                    {
                        if(map[i,j]!=0)
                            g.FillRectangle(skyBluePenBrush, new Rectangle(padding + i * side_len, padding + j * side_len, side_len, side_len));
                        
                        g.DrawRectangle(skyBluePen, new Rectangle(padding + i * side_len, padding + j * side_len, side_len, side_len));
                    }
                }
            SolidBrush startBrush = new SolidBrush(Color.Red);
            g.FillRectangle(startBrush, new Rectangle(padding + start_p.X * side_len, padding + start_p.Y * side_len, side_len, side_len));

            SolidBrush endBrush = new SolidBrush(Color.Black);
            g.FillRectangle(endBrush, new Rectangle(padding + end_p.X * side_len, padding + end_p.Y * side_len, side_len, side_len));
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            current_x = (e.X - w_num) / side_len;
            current_y = (e.Y - h_num) / side_len;

            if (current_x < 0 || current_y < 0 || current_x >= w_num || current_y >= h_num)
                return;

            path_len = -1;

            if (setStart_flag)
            {
                start_p.X = current_x; start_p.Y = current_y;
                setStart_flag = false;
                btnSetStart.BackColor = Color.White;
            }
            else if (setEnd_flag)
            {
                end_p.X = current_x; end_p.Y = current_y;
                setEnd_flag = false;
                btnSetEnd.BackColor = Color.White;
            }
            else
            {
                mouse_down_flag = true;

                if (map[current_x, current_y] == 0)
                {
                    edit_flag = true;
                    map[current_x, current_y] = 1;
                }
                else
                {
                    edit_flag = false;
                    map[current_x, current_y] = 0;
                }
            }

            Invalidate();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouse_down_flag)
            {
                current_x = (e.X - w_num) / side_len;
                current_y = (e.Y - h_num) / side_len;
                if (current_x < 0 || current_y < 0 || current_x >= w_num || current_y >= h_num)
                    return;

                if (edit_flag)
                    map[current_x, current_y] = 1;
                else
                    map[current_x, current_y] = 0;

                Invalidate();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            for(int i=0; i<w_num; i++)
                for(int j=0; j<h_num; j++)
                {
                    map[i, j] = 0;
                }
            path_len = -1;
            Invalidate();
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            mouse_down_flag = false;
        }

        public void readData_to_map()
        {
            int nextline = 0;
            int nextword = 0;
            foreach (char v in frm2.map_data)
            {
                
                if (v == '\n')
                {
                    nextword = 0;
                    nextline++;
                }
                else
                {
                    map[nextword++, nextline] = int.Parse(v.ToString());
                }
                if (nextline >= w_num || nextword >= h_num)
                    continue;
            }

            Invalidate();
        }


        public class Tile
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int F { get { return G + H; } }
            public int G { get; private set; } // Start ~ Current
            public int H { get; private set; } // Current ~ End
            public Tile Parent { get; private set; }
            public void Execute(Tile parent, Tile endTile)
            {
                Parent = parent;
                G = CalcGValue(parent, this);
                int diffX = Math.Abs(endTile.X - X);
                int diffY = Math.Abs(endTile.Y - Y);
                H = (diffX + diffY) * 10;
            }
            public static int CalcGValue(Tile parent, Tile current)
            {
                int diffX = Math.Abs(parent.X - current.X);
                int diffY = Math.Abs(parent.Y - current.Y);
                int value = 10;
                if (diffX == 1 && diffY == 1)
                {
                    value = 14;
                }
                return parent.G + value;
            }
        }

        public static int minimumDistance(int numRows, int numColumns, Point start, Point end, int[,] area)
        {
            int result = -1; // distance
            int failResult = -1;
            int obstacle = 1;
            List<List<Tile>> tiles = new List<List<Tile>>();
            List<Tile> openList = new List<Tile>();
            List<Tile> closeList = new List<Tile>();
            path = new List<Tile>();
            for (int i = 0; i < area.GetLength(0); i++)
            {
                List<Tile> t = new List<Tile>();
                for (int j = 0; j < area.GetLength(1); j++)
                {
                    Tile temp = new Tile();
                    temp.X = i;
                    temp.Y = j;
                    t.Add(temp);
                }
                tiles.Add(t);
            }
            Tile startTile = tiles[start.X][start.Y];
            Tile targetTile = tiles[end.X][end.Y];

            openList.Add(startTile);

            Tile currentTile = null;
            do
            {
                if (openList.Count == 0)
                {
                    break;
                }
                currentTile = openList.OrderBy(o => o.F).First();
                openList.Remove(currentTile);
                closeList.Add(currentTile);
                if (currentTile == targetTile)
                {
                    break;
                }
                for (int i = 0; i < area.GetLength(0); i++)
                {
                    for (int j = 0; j < area.GetLength(1); j++)
                    {
                        // 8 way
                        bool near = (Math.Abs(currentTile.X - tiles[i][j].X) <= 1)
                                 && (Math.Abs(currentTile.Y - tiles[i][j].Y) <= 1);
                        //// 4 way
                        //bool near = (Math.Abs(currentTile.X - tiles[i][j].X) <= 1)
                        //         && (Math.Abs(currentTile.Y - tiles[i][j].Y) <= 1)
                        //         && (currentTile.Y == tiles[i][j].Y || currentTile.X == tiles[i][j].X);
                        if (area[i, j] == obstacle
                         || closeList.Contains(tiles[i][j])
                         || (!near))
                        {
                            continue;
                        }
                        if (!openList.Contains(tiles[i][j]))
                        {
                            openList.Add(tiles[i][j]);
                            tiles[i][j].Execute(currentTile, targetTile);
                        }
                        else
                        {
                            if (Tile.CalcGValue(currentTile, tiles[i][j]) < tiles[i][j].G)
                            {
                                tiles[i][j].Execute(currentTile, targetTile);
                            }
                        }
                    }
                }
            } while (currentTile != null);
            if (currentTile != targetTile)
            {
                // can not found root
                return failResult;
            }
            do
            {
                path.Add(currentTile);
                currentTile = currentTile.Parent;
            }
            while (currentTile != null);
            path.Reverse();
            
            result = path.Count - 1;
            return result;
        }

        private void my_timer()
        {
            timer = 1;
            while (path_len!=-1 && timer != path.Count)
            {
                Invalidate();
                Thread.Sleep(100);
                timer++;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            path_len = minimumDistance(map.GetLength(0), map.GetLength(1), start_p, end_p, map);
            
            if (path_len == -1)
            {
                MessageBox.Show("가능한 경로가 없음.");
                return;
            }
            Thread thread = new Thread(() => my_timer());
            thread.Start();

            Invalidate();
        }

        private void btnSetStart_Click(object sender, EventArgs e)
        {
            setStart_flag = true;
            btnSetStart.BackColor = Color.Gold;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            setEnd_flag = true;
            btnSetEnd.BackColor = Color.Gold;
        }

        public void writeData_to_map()
        {
            frm2.map_data = "";
            for (int i = 0; i < w_num; i++)
            {
                for (int j = 0; j < h_num; j++)
                {
                    frm2.map_data += map[j, i].ToString();
                }
                frm2.map_data += "\n";
            }
        }

        private void FileIO_Click(object sender, EventArgs e)
        {
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.Name == "Form2")
                {
                    frm.Activate();
                    return;
                }
            }

            frm2 = new Form2();
            frm2.Owner = this;
            frm2.parent = this;

            frm2.Show();
        }


    }
}

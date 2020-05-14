using System;
using System.Collections.Generic;
using System.Numerics;

namespace RandomMap_Sample
{
    struct Node
    {
        public bool mbeast;
        public bool mbsouth;
    };
    struct NodeConnection
    {
        public bool mbempty;
        public Node mnode;
    };
    public struct Coord
    {
        public int mx;
        public int my;
    };
    struct Cardinalpoint
    {
        public bool mbnorth;
        public bool mbsouth;
        public bool mbeast;
        public bool mbwest;
    };
    struct MasterNodeInfo
    {
        public bool mbcheaked;
        public bool mbMasterNode;
        public Coord mmasterNodeCoord;
    };
    public struct MapPattrnInfo
    {
        public int mleftPoint;
        public int mrightpoint;
    };
    public enum NodeTileBaseInfo
    {
        //  None
        None = 0,

        //  Clode = 4, Open = 0
        Close4nsew_Open0 = 1,

        //  Clode = 3, Open = 1
        Close3new_Open1s = 2,
        Close3nse_Open1w = 3,
        Close3sew_Open1n = 4,
        Close3nsw_Open1e = 5,

        //  Clode = 2, Open = 2
        Close2nw_Open2se = 6,
        Close2ne_Open2sw = 7,
        Close2se_Open2nw = 8,
        Close2sw_Open2ne = 9,

        //  Clode = 1, Open = 3
        Close1n_Open3sew = 10,
        Close1e_Open3nsw = 11,
        Close1s_Open3new = 12,
        Close1w_Open3nse = 13,

        //  Clode = 0, Open = 4
        Close0_Open4nsew = 14
    };

    class NodeManager
    {
        private NodeConnection[,] mnodeConnection;
        private Node[,] mnodePath;
        private int mlength_x, mlength_y;
        private Random random;

        private int mnodeChance, mnodeConChance, mnodePathChance, mdenominator;

        private List<MapPattrnInfo>[,] mmapPattrnInfo;

        private List<long>[] mnodeLinkInfo;

        private NodeTileBaseInfo[,] mnodeTileBaseInfo;

        private MasterNodeInfo[,] mmasterNodeInfo;

        //  public method
        public NodeManager(in int length_x, in int length_y, in int nodeChance, in int nodeConChance, in int nodePathChance, in int denominator, in int mapIndex)
        {
            mlength_x = length_x;
            mlength_y = length_y;
            random = new Random();

            mnodeChance = nodeChance;
            mnodeConChance = nodeConChance;
            mnodePathChance = nodePathChance;
            mdenominator = denominator;

            mnodeConnection = new NodeConnection[mlength_x, mlength_y];
            mnodePath = new Node[mlength_x, mlength_y];

            mnodeLinkInfo = new List<long>[mlength_x * mlength_y];
            for (int i = 0; i < mlength_x * mlength_y; i++)
            {
                mnodeLinkInfo[i] = new List<long>();
            }

            mmapPattrnInfo = new List<MapPattrnInfo>[9, mlength_y];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < mlength_y; j++)
                {
                    mmapPattrnInfo[i, j] = new List<MapPattrnInfo>();
                }
            }

            mnodeTileBaseInfo = new NodeTileBaseInfo[mlength_x, mlength_y];
            mmasterNodeInfo = new MasterNodeInfo[mlength_x, mlength_y];

            SetMapPattenInfo();
            MakeNodeConnection(mapIndex);
            MakeNodePath();
            MakeCompleteNodeInfo();
            MakeNodeTileBaseInfo();
            MakeMasterNodeInfo();
        }
        public void DrawMap()
        {
            for (int i = 0; i < mlength_y; i++)
            {
                for (int j = 0; j < mlength_x; j++)
                {
                    if (mnodeConnection[j, i].mbempty == true)
                    {
                        Console.Write("□");
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                    if (mnodeConnection[j, i].mnode.mbeast == true)
                    {
                        Console.Write("□");
                    }
                    else if (mnodePath[j, i].mbeast == true)
                    {
                        Console.Write("--");
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                }
                Console.WriteLine("");

                for (int j = 0; j < mlength_x; j++)
                {
                    if (mnodeConnection[j, i].mnode.mbsouth == true)
                    {
                        Console.Write("□  ");
                    }
                    else if (mnodePath[j, i].mbsouth == true)
                    {
                        Console.Write("|   ");
                    }
                    else
                    {
                        Console.Write("    ");
                    }
                }
                Console.WriteLine("");
            }
        }
        public void PrintGraph()
        {
            for (int i = 0; i < mlength_x * mlength_y; i++)
            {
                Console.Write("Node index : " + i + "\t||\t");
                for (int j = 0; j < mnodeLinkInfo[i].Count; j++)
                {
                    Console.Write(mnodeLinkInfo[i][j] + "\t");
                }

                Console.WriteLine("");
            }
        }
        public void DrawMap_TestVer(in int index)
        {
            for (int i = 0; i < mlength_y; i++)
            {
                for (int j = 0; j < mlength_x; j++)
                {
                    bool bisDraw = false;
                    for (int temp = 0; temp < mmapPattrnInfo[index, i].Count; temp++)
                    {
                        if (j >= mmapPattrnInfo[index, i][temp].mleftPoint && j <= mmapPattrnInfo[index, i][temp].mrightpoint)
                        {
                            bisDraw = true;
                            Console.Write("□");

                            break;
                        }
                    }

                    if (bisDraw == false)
                    {
                        Console.Write("  ");
                    }
                }

                Console.WriteLine("");
            }
        }

        //  private method
        private void MakeNodeConnection(in int mapIndex)
        {
            for (int coord_y = 0; coord_y < mlength_y; coord_y++)
            {
                for (int coord_x = 0; coord_x < mlength_x; coord_x++)
                {
                    //  Map Shape Cheak
                    for (int temp = 0; temp < mmapPattrnInfo[mapIndex, coord_y].Count; temp++)
                    {
                        if (coord_x >= mmapPattrnInfo[mapIndex, coord_y][temp].mleftPoint && coord_x <= mmapPattrnInfo[mapIndex, coord_y][temp].mrightpoint)
                        {
                            if (GetPercent(mnodeChance, mdenominator) == true)
                            {
                                mnodeConnection[coord_x, coord_y].mbempty = true;

                                if (coord_x == 0 && coord_y == 0)
                                {
                                    mnodeConnection[coord_x, coord_y].mnode.mbeast = false;
                                    mnodeConnection[coord_x, coord_y].mnode.mbsouth = false;
                                }
                                else if (coord_x == 0 && coord_y != 0)
                                {
                                    if (mnodeConnection[coord_x, coord_y - 1].mbempty == true && GetPercent(mnodeConChance, mdenominator) == true)
                                    {
                                        mnodeConnection[coord_x, coord_y - 1].mnode.mbsouth = true;
                                    }
                                    else
                                    {
                                        mnodeConnection[coord_x, coord_y - 1].mnode.mbsouth = false;
                                    }
                                }
                                else if (coord_y == 0 && coord_x != 0)
                                {
                                    if (mnodeConnection[coord_x - 1, coord_y].mbempty == true && GetPercent(mnodeConChance, mdenominator) == true)
                                    {
                                        mnodeConnection[coord_x - 1, coord_y].mnode.mbeast = true;
                                    }
                                    else
                                    {
                                        mnodeConnection[coord_x - 1, coord_y].mnode.mbeast = false;
                                    }
                                }
                                else if (coord_x == mlength_x - 1 && coord_y != mlength_y - 1)
                                {
                                    mnodeConnection[coord_x, coord_y - 1].mnode.mbeast = false;
                                    if (mnodeConnection[coord_x, coord_y - 1].mbempty == true && GetPercent(mnodeConChance, mdenominator) == true)
                                    {
                                        mnodeConnection[coord_x, coord_y - 1].mnode.mbsouth = true;
                                    }
                                    else
                                    {
                                        mnodeConnection[coord_x, coord_y - 1].mnode.mbsouth = false;
                                    }
                                }
                                else if (coord_y == mlength_y - 1 && coord_x != mlength_x - 1)
                                {
                                    mnodeConnection[coord_x - 1, coord_y].mnode.mbsouth = false;
                                    if (mnodeConnection[coord_x - 1, coord_y].mbempty == true && GetPercent(mnodeConChance, mdenominator) == true)
                                    {
                                        mnodeConnection[coord_x - 1, coord_y].mnode.mbeast = true;
                                    }
                                    else
                                    {
                                        mnodeConnection[coord_x - 1, coord_y].mnode.mbeast = false;
                                    }
                                }
                                else if (coord_x == mlength_x - 1 && coord_y == mlength_y - 1)
                                {
                                    mnodeConnection[coord_x, coord_y].mnode.mbeast = false;
                                    mnodeConnection[coord_x, coord_y].mnode.mbsouth = false;
                                }
                                else
                                {
                                    if (mnodeConnection[coord_x, coord_y - 1].mbempty == true && GetPercent(mnodeConChance, mdenominator) == true)
                                    {
                                        mnodeConnection[coord_x, coord_y - 1].mnode.mbsouth = true;
                                    }
                                    else
                                    {
                                        mnodeConnection[coord_x, coord_y - 1].mnode.mbsouth = false;
                                    }

                                    if (mnodeConnection[coord_x - 1, coord_y].mbempty == true && GetPercent(mnodeConChance, mdenominator) == true)
                                    {
                                        mnodeConnection[coord_x - 1, coord_y].mnode.mbeast = true;
                                    }
                                    else
                                    {
                                        mnodeConnection[coord_x - 1, coord_y].mnode.mbeast = false;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }
        private void MakeNodePath()
        {
            for (int coord_y = 0; coord_y < mlength_y; coord_y++)
            {
                for (int coord_x = 0; coord_x < mlength_x; coord_x++)
                {
                    if (coord_x == 0 && coord_y == 0)
                    {

                    }
                    else if (coord_x == 0 && coord_y != 0)
                    {
                        if (mnodeConnection[coord_x, coord_y].mbempty == true && mnodeConnection[coord_x, coord_y - 1].mbempty == true && mnodeConnection[coord_x, coord_y - 1].mnode.mbsouth == false && GetPercent(mnodePathChance, mdenominator) == true)
                        {
                            mnodePath[coord_x, coord_y - 1].mbsouth = true;
                        }
                        else
                        {
                            mnodePath[coord_x, coord_y - 1].mbsouth = false;
                        }
                    }
                    else if (coord_x != 0 && coord_y == 0)
                    {
                        if (mnodeConnection[coord_x, coord_y].mbempty == true && mnodeConnection[coord_x - 1, coord_y].mbempty == true && mnodeConnection[coord_x - 1, coord_y].mnode.mbeast == false && GetPercent(mnodePathChance, mdenominator) == true)
                        {
                            mnodePath[coord_x - 1, coord_y].mbeast = true;
                        }
                        else
                        {
                            mnodePath[coord_x - 1, coord_y].mbeast = false;
                        }
                    }
                    else
                    {
                        if (mnodeConnection[coord_x, coord_y].mbempty == true && mnodeConnection[coord_x, coord_y - 1].mbempty == true && mnodeConnection[coord_x, coord_y - 1].mnode.mbsouth == false && GetPercent(mnodePathChance, mdenominator) == true)
                        {
                            mnodePath[coord_x, coord_y - 1].mbsouth = true;
                        }
                        else
                        {
                            mnodePath[coord_x, coord_y - 1].mbsouth = false;
                        }

                        if (mnodeConnection[coord_x, coord_y].mbempty == true && mnodeConnection[coord_x - 1, coord_y].mbempty == true && mnodeConnection[coord_x - 1, coord_y].mnode.mbeast == false && GetPercent(mnodePathChance, mdenominator) == true)
                        {
                            mnodePath[coord_x - 1, coord_y].mbeast = true;
                        }
                        else
                        {
                            mnodePath[coord_x - 1, coord_y].mbeast = false;
                        }
                    }
                }
            }
        }
        private void MakeCompleteNodeInfo()
        {
            for (int coord_y = 0; coord_y < mlength_y; coord_y++)
            {
                for (int coord_x = 0; coord_x < mlength_x; coord_x++)
                {
                    int arrIndex = coord_y * mlength_x + coord_x;

                    if (coord_x == 0 && coord_y == 0)
                    {
                        if (mnodeConnection[coord_x, coord_y].mnode.mbeast == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + 1);
                        }
                        if (mnodeConnection[coord_x, coord_y].mnode.mbsouth == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + mlength_x);
                        }

                        if (mnodePath[coord_x, coord_y].mbeast == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + 1);
                        }
                        if (mnodePath[coord_x, coord_y].mbsouth == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + mlength_x);
                        }
                    }
                    else if (coord_x == 0 && coord_y != 0)
                    {
                        if (mnodeConnection[coord_x, coord_y].mnode.mbeast == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + 1);
                        }
                        if (mnodeConnection[coord_x, coord_y].mnode.mbsouth == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + mlength_x);
                        }
                        if (mnodeConnection[coord_x, coord_y - 1].mnode.mbsouth == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex - mlength_x);
                        }

                        if (mnodePath[coord_x, coord_y].mbeast == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + 1);
                        }
                        if (mnodePath[coord_x, coord_y].mbsouth == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + mlength_x);
                        }
                        if (mnodePath[coord_x, coord_y - 1].mbsouth == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex - mlength_x);
                        }
                    }
                    else if (coord_x != 0 && coord_y == 0)
                    {
                        if (mnodeConnection[coord_x, coord_y].mnode.mbeast == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + 1);
                        }
                        if (mnodeConnection[coord_x, coord_y].mnode.mbsouth == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + mlength_x);
                        }
                        if (mnodeConnection[coord_x - 1, coord_y].mnode.mbeast == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex - 1);
                        }

                        if (mnodePath[coord_x, coord_y].mbeast == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + 1);
                        }
                        if (mnodePath[coord_x, coord_y].mbsouth == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + mlength_x);
                        }
                        if (mnodePath[coord_x - 1, coord_y].mbeast == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex - 1);
                        }
                    }
                    else
                    {
                        if (mnodeConnection[coord_x, coord_y].mnode.mbeast == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + 1);
                        }
                        if (mnodeConnection[coord_x, coord_y].mnode.mbsouth == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + mlength_x);
                        }
                        if (mnodeConnection[coord_x, coord_y - 1].mnode.mbsouth == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex - mlength_x);
                        }
                        if (mnodeConnection[coord_x - 1, coord_y].mnode.mbeast == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex - 1);
                        }

                        if (mnodePath[coord_x, coord_y].mbeast == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + 1);
                        }
                        if (mnodePath[coord_x, coord_y].mbsouth == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex + mlength_x);
                        }
                        if (mnodePath[coord_x, coord_y - 1].mbsouth == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex - mlength_x);
                        }
                        if (mnodePath[coord_x - 1, coord_y].mbeast == true)
                        {
                            mnodeLinkInfo[arrIndex].Add(arrIndex - 1);
                        }
                    }
                }
            }
        }
        private void MakeNodeTileBaseInfo()
        {
            for (int coord_y = 0; coord_y < mlength_y; coord_y++)
            {
                for (int coord_x = 0; coord_x < mlength_x; coord_x++)
                {
                    long arrIndex = GetArrIndex(coord_x, coord_y);

                    Cardinalpoint nodeCardinalpoint = new Cardinalpoint();
                    GetCardinalpoint(ref nodeCardinalpoint, coord_x, coord_y, arrIndex);
                    GetTileBaseInfo(nodeCardinalpoint, coord_x, coord_y);
                }
            }
        }
        private void MakeMasterNodeInfo()
        {
            for (int coord_y = 0; coord_y < mlength_y; coord_y++)
            {
                for (int coord_x = 0; coord_x < mlength_x; coord_x++)
                {
                    if (mmasterNodeInfo[coord_x, coord_y].mbcheaked == false)
                    {
                        if (mnodeConnection[coord_x, coord_y].mbempty == false)
                        {
                            mmasterNodeInfo[coord_x, coord_y].mbcheaked = true;
                            mmasterNodeInfo[coord_x, coord_y].mbMasterNode = false;
                            mmasterNodeInfo[coord_x, coord_y].mmasterNodeCoord.mx = -1;
                            mmasterNodeInfo[coord_x, coord_y].mmasterNodeCoord.my = -1;
                        }
                        else
                        {
                            Coord masterNodeCoord;
                            masterNodeCoord.mx = coord_x;
                            masterNodeCoord.my = coord_y;
                            SearchMasterNode(coord_x, coord_y, masterNodeCoord, true);
                        }
                    }
                }
            }
        }
        private long GetArrIndex(in int coord_x, in int coord_y)
        {
            return coord_y * mlength_x + coord_x;
        }
        private bool GetPercent(in int molecular, in int denominator)
        {
            if (random.Next(1, denominator) <= molecular)
            {
                return true;
            }
            return false;
        }
        private void SearchMasterNode(in int coord_x, in int coord_y, in Coord masterNodeCoord, in bool bfirstCall)
        {
            long arrIndex = GetArrIndex(coord_x, coord_y);
            int flagNum = mnodeLinkInfo[arrIndex].Count;

            Cardinalpoint direction = new Cardinalpoint();
            for (int i = 0; i < mnodeLinkInfo[arrIndex].Count; i++)
            {
                if (mnodeLinkInfo[arrIndex][i] == arrIndex - mlength_x)
                {
                    direction.mbnorth = true;
                }
                else if (mnodeLinkInfo[arrIndex][i] == arrIndex + mlength_x)
                {
                    direction.mbsouth = true;
                }
                else if (mnodeLinkInfo[arrIndex][i] == arrIndex - 1)
                {
                    direction.mbwest = true;
                }
                else if (mnodeLinkInfo[arrIndex][i] == arrIndex + 1)
                {
                    direction.mbeast = true;
                }
            }

            //  flagNum Setting
            SetFlagNum_First(coord_x, coord_y, ref flagNum, ref direction);
            SetFlagNum_Second(arrIndex, ref flagNum, coord_x, coord_y, ref direction);

            mmasterNodeInfo[coord_x, coord_y].mbcheaked = true;
            if (bfirstCall == true)
            {
                mmasterNodeInfo[coord_x, coord_y].mbMasterNode = true;
            }
            else
            {
                mmasterNodeInfo[coord_x, coord_y].mbMasterNode = false;
            }
            mmasterNodeInfo[coord_x, coord_y].mmasterNodeCoord.mx = masterNodeCoord.mx;
            mmasterNodeInfo[coord_x, coord_y].mmasterNodeCoord.my = masterNodeCoord.my;

            if (flagNum == 0)
            {
                return;
            }
            else
            {
                if (direction.mbnorth == true)
                {
                    SearchMasterNode(coord_x, coord_y - 1, masterNodeCoord, false);
                }
                if (direction.mbsouth == true)
                {
                    SearchMasterNode(coord_x, coord_y + 1, masterNodeCoord, false);
                }
                if (direction.mbwest == true)
                {
                    SearchMasterNode(coord_x - 1, coord_y, masterNodeCoord, false);
                }
                if (direction.mbeast == true)
                {
                    SearchMasterNode(coord_x + 1, coord_y, masterNodeCoord, false);
                }
            }
        }
        private void SetFlagNum_First(in int coord_x, in int coord_y, ref int flagNum, ref Cardinalpoint direction)
        {
            if (coord_x == 0 && coord_y == 0)
            {
                direction.mbnorth = false;
                direction.mbwest = false;

                if (mnodePath[coord_x, coord_y].mbeast == true)
                {
                    flagNum--;
                    direction.mbeast = false;
                }
                if (mnodePath[coord_x, coord_y].mbsouth == true)
                {
                    flagNum--;
                    direction.mbsouth = false;
                }
            }
            else if (coord_x == 0 && coord_y != 0)
            {
                direction.mbwest = false;

                if (mnodePath[coord_x, coord_y].mbeast == true)
                {
                    flagNum--;
                    direction.mbeast = false;
                }
                if (mnodePath[coord_x, coord_y].mbsouth == true)
                {
                    flagNum--;
                    direction.mbsouth = false;
                }
                if (mnodePath[coord_x, coord_y - 1].mbsouth == true)
                {
                    flagNum--;
                    direction.mbnorth = false;
                }
            }
            else if (coord_x != 0 && coord_y == 0)
            {
                if (mnodePath[coord_x, coord_y].mbeast == true)
                {
                    flagNum--;
                    direction.mbeast = false;
                }
                if (mnodePath[coord_x, coord_y].mbsouth == true)
                {
                    flagNum--;
                    direction.mbsouth = false;
                }
                if (mnodePath[coord_x - 1, coord_y].mbeast == true)
                {
                    flagNum--;
                    direction.mbwest = false;
                }
            }
            else
            {
                if (mnodePath[coord_x, coord_y].mbeast == true)
                {
                    flagNum--;
                    direction.mbeast = false;
                }
                if (mnodePath[coord_x, coord_y].mbsouth == true)
                {
                    flagNum--;
                    direction.mbsouth = false;
                }
                if (mnodePath[coord_x, coord_y - 1].mbsouth == true)
                {
                    flagNum--;
                    direction.mbnorth = false;
                }
                if (mnodePath[coord_x - 1, coord_y].mbeast == true)
                {
                    flagNum--;
                    direction.mbwest = false;
                }
            }
        }
        private void SetFlagNum_Second(in long arrIndex, ref int flagNum, in int coord_x, in int coord_y, ref Cardinalpoint direction)
        {
            for (int i = 0; i < mnodeLinkInfo[arrIndex].Count; i++)
            {
                //  North
                if (mnodeLinkInfo[arrIndex][i] == arrIndex - mlength_x && mmasterNodeInfo[coord_x, coord_y - 1].mbcheaked == true)
                {
                    flagNum--;
                    direction.mbnorth = false;
                }

                //  South
                else if (mnodeLinkInfo[arrIndex][i] == arrIndex + mlength_x && mmasterNodeInfo[coord_x, coord_y + 1].mbcheaked == true)
                {
                    flagNum--;
                    direction.mbsouth = false;
                }

                //  West
                else if (mnodeLinkInfo[arrIndex][i] == arrIndex - 1 && mmasterNodeInfo[coord_x - 1, coord_y].mbcheaked == true)
                {
                    flagNum--;
                    direction.mbwest = false;
                }

                //  East
                else if (mnodeLinkInfo[arrIndex][i] == arrIndex + 1 && mmasterNodeInfo[coord_x + 1, coord_y].mbcheaked == true)
                {
                    flagNum--;
                    direction.mbeast = false;
                }
            }
        }
        private void GetCardinalpoint(ref Cardinalpoint cardinalpoint, in int coord_x, in int coord_y, in long arrIndex)
        {
            if (coord_x == 0 && coord_y == 0)
            {
                //  South
                if (mnodeConnection[coord_x, coord_y].mnode.mbsouth == true)
                {
                    cardinalpoint.mbsouth = true;
                }
                //  East
                else if (mnodeConnection[coord_x, coord_y].mnode.mbeast == true)
                {
                    cardinalpoint.mbeast = true;
                }
            }
            else if (coord_x != 0 && coord_y == 0)
            {
                //  South
                if (mnodeConnection[coord_x, coord_y].mnode.mbsouth == true)
                {
                    cardinalpoint.mbsouth = true;
                }
                //  East
                else if (mnodeConnection[coord_x, coord_y].mnode.mbeast == true)
                {
                    cardinalpoint.mbeast = true;
                }
                //  West
                else if (mnodeConnection[coord_x - 1, coord_y].mnode.mbeast == true)
                {
                    cardinalpoint.mbwest = true;
                }
            }
            else if (coord_x == 0 && coord_y != 0)
            {
                //  South
                if (mnodeConnection[coord_x, coord_y].mnode.mbsouth == true)
                {
                    cardinalpoint.mbsouth = true;
                }
                //  North
                else if (mnodeConnection[coord_x, coord_y - 1].mnode.mbsouth == true)
                {
                    cardinalpoint.mbnorth = true;
                }
                //  East
                else if (mnodeConnection[coord_x, coord_y].mnode.mbeast == true)
                {
                    cardinalpoint.mbeast = true;
                }
            }
            else
            {
                //  South
                if (mnodeConnection[coord_x, coord_y].mnode.mbsouth == true)
                {
                    cardinalpoint.mbsouth = true;
                }
                //  North
                else if (mnodeConnection[coord_x, coord_y - 1].mnode.mbsouth == true)
                {
                    cardinalpoint.mbnorth = true;
                }
                //  East
                else if (mnodeConnection[coord_x, coord_y].mnode.mbeast == true)
                {
                    cardinalpoint.mbeast = true;
                }
                //  West
                else if (mnodeConnection[coord_x - 1, coord_y].mnode.mbeast == true)
                {
                    cardinalpoint.mbwest = true;
                }
            }
        }
        private void GetTileBaseInfo(in Cardinalpoint cardinalpoint, in int coord_x, in int coord_y)
        {
            // Node
            if (cardinalpoint.mbnorth == false && cardinalpoint.mbsouth == false && cardinalpoint.mbwest == false && cardinalpoint.mbeast == false && mnodeConnection[coord_x, coord_y].mbempty == false)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.None;
            }

            //  Close = 4 & Open = 0
            if (cardinalpoint.mbnorth == false && cardinalpoint.mbsouth == false && cardinalpoint.mbwest == false && cardinalpoint.mbeast == false && mnodeConnection[coord_x, coord_y].mbempty == true)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close4nsew_Open0;
            }

            //  Close = 3 & Open = 1
            if (cardinalpoint.mbnorth == false && cardinalpoint.mbsouth == true && cardinalpoint.mbwest == false && cardinalpoint.mbeast == false)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close3new_Open1s;
            }
            else if (cardinalpoint.mbnorth == false && cardinalpoint.mbsouth == false && cardinalpoint.mbwest == true && cardinalpoint.mbeast == false)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close3nse_Open1w;
            }
            else if (cardinalpoint.mbnorth == true && cardinalpoint.mbsouth == false && cardinalpoint.mbwest == false && cardinalpoint.mbeast == false)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close3sew_Open1n;
            }
            else if (cardinalpoint.mbnorth == false && cardinalpoint.mbsouth == false && cardinalpoint.mbwest == false && cardinalpoint.mbeast == true)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close3nsw_Open1e;
            }

            //  Close = 2 & Open = 2
            if (cardinalpoint.mbnorth == true && cardinalpoint.mbsouth == false && cardinalpoint.mbwest == false && cardinalpoint.mbeast == true)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close2sw_Open2ne;
            }
            else if (cardinalpoint.mbnorth == false && cardinalpoint.mbsouth == true && cardinalpoint.mbwest == false && cardinalpoint.mbeast == true)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close2nw_Open2se;
            }
            else if (cardinalpoint.mbnorth == false && cardinalpoint.mbsouth == true && cardinalpoint.mbwest == true && cardinalpoint.mbeast == false)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close2ne_Open2sw;
            }
            else if (cardinalpoint.mbnorth == true && cardinalpoint.mbsouth == false && cardinalpoint.mbwest == true && cardinalpoint.mbeast == false)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close2se_Open2nw;
            }

            //  Close = 1 & Open = 3
            if (cardinalpoint.mbnorth == false && cardinalpoint.mbsouth == true && cardinalpoint.mbwest == true && cardinalpoint.mbeast == true)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close1n_Open3sew;
            }
            else if (cardinalpoint.mbnorth == true && cardinalpoint.mbsouth == true && cardinalpoint.mbwest == true && cardinalpoint.mbeast == false)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close1e_Open3nsw;
            }
            else if (cardinalpoint.mbnorth == true && cardinalpoint.mbsouth == false && cardinalpoint.mbwest == true && cardinalpoint.mbeast == true)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close1s_Open3new;
            }
            else if (cardinalpoint.mbnorth == true && cardinalpoint.mbsouth == true && cardinalpoint.mbwest == false && cardinalpoint.mbeast == true)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close1w_Open3nse;
            }

            //  Close = 0 & Open = 4
            if (cardinalpoint.mbnorth == true && cardinalpoint.mbsouth == true && cardinalpoint.mbwest == true && cardinalpoint.mbeast == true)
            {
                mnodeTileBaseInfo[coord_x, coord_y] = NodeTileBaseInfo.Close0_Open4nsew;
            }
        }
        private void SetMapPattenInfo()
        {
            for (int index = 0; index < 9; index++)
            {
                MapPattrnInfo tempMapPattrnInfo = new MapPattrnInfo();

                //  First Map Pattrn
                if (index == 0)
                {
                    tempMapPattrnInfo.mleftPoint = 41;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 6].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 8;
                    mmapPattrnInfo[index, 7].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 26;
                    tempMapPattrnInfo.mrightpoint = 34;
                    mmapPattrnInfo[index, 7].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 40;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 7].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 9;
                    mmapPattrnInfo[index, 8].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 25;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 8].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 8;
                    mmapPattrnInfo[index, 9].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 15;
                    tempMapPattrnInfo.mrightpoint = 18;
                    mmapPattrnInfo[index, 9].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 23;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 9].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 7;
                    mmapPattrnInfo[index, 10].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 12;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 10].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 6;
                    mmapPattrnInfo[index, 11].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 11;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 11].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 6;
                    mmapPattrnInfo[index, 12].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 11;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 12].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 7;
                    mmapPattrnInfo[index, 13].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 10;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 13].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 7;
                    mmapPattrnInfo[index, 14].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 10;
                    tempMapPattrnInfo.mrightpoint = 20;
                    mmapPattrnInfo[index, 14].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 26;
                    tempMapPattrnInfo.mrightpoint = 30;
                    mmapPattrnInfo[index, 14].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 35;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 14].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 8;
                    mmapPattrnInfo[index, 15].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 10;
                    tempMapPattrnInfo.mrightpoint = 16;
                    mmapPattrnInfo[index, 15].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 38;
                    tempMapPattrnInfo.mrightpoint = 44;
                    mmapPattrnInfo[index, 15].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 14;
                    mmapPattrnInfo[index, 16].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 39;
                    tempMapPattrnInfo.mrightpoint = 44;
                    mmapPattrnInfo[index, 16].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 13;
                    mmapPattrnInfo[index, 17].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 38;
                    tempMapPattrnInfo.mrightpoint = 43;
                    mmapPattrnInfo[index, 17].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 13;
                    mmapPattrnInfo[index, 18].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 38;
                    tempMapPattrnInfo.mrightpoint = 43;
                    mmapPattrnInfo[index, 18].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 12;
                    mmapPattrnInfo[index, 19].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 37;
                    tempMapPattrnInfo.mrightpoint = 43;
                    mmapPattrnInfo[index, 19].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 12;
                    mmapPattrnInfo[index, 20].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 37;
                    tempMapPattrnInfo.mrightpoint = 43;
                    mmapPattrnInfo[index, 20].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 11;
                    mmapPattrnInfo[index, 21].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 37;
                    tempMapPattrnInfo.mrightpoint = 43;
                    mmapPattrnInfo[index, 21].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 10;
                    mmapPattrnInfo[index, 22].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 36;
                    tempMapPattrnInfo.mrightpoint = 43;
                    mmapPattrnInfo[index, 22].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 10;
                    mmapPattrnInfo[index, 23].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 36;
                    tempMapPattrnInfo.mrightpoint = 44;
                    mmapPattrnInfo[index, 23].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 9;
                    mmapPattrnInfo[index, 24].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 36;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 24].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 9;
                    mmapPattrnInfo[index, 25].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 35;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 25].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 9;
                    mmapPattrnInfo[index, 26].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 35;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 26].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 9;
                    mmapPattrnInfo[index, 27].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 34;
                    tempMapPattrnInfo.mrightpoint = 47;
                    mmapPattrnInfo[index, 27].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 9;
                    mmapPattrnInfo[index, 28].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 34;
                    tempMapPattrnInfo.mrightpoint = 47;
                    mmapPattrnInfo[index, 28].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 9;
                    mmapPattrnInfo[index, 29].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 33;
                    tempMapPattrnInfo.mrightpoint = 47;
                    mmapPattrnInfo[index, 29].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 11;
                    mmapPattrnInfo[index, 30].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 32;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 30].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 12;
                    mmapPattrnInfo[index, 31].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 31;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 31].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 14;
                    mmapPattrnInfo[index, 32].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 30;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 32].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 15;
                    mmapPattrnInfo[index, 33].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 30;
                    tempMapPattrnInfo.mrightpoint = 37;
                    mmapPattrnInfo[index, 33].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 41;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 33].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 16;
                    mmapPattrnInfo[index, 34].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 29;
                    tempMapPattrnInfo.mrightpoint = 35;
                    mmapPattrnInfo[index, 34].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 16;
                    mmapPattrnInfo[index, 35].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 28;
                    tempMapPattrnInfo.mrightpoint = 35;
                    mmapPattrnInfo[index, 35].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 17;
                    mmapPattrnInfo[index, 36].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 27;
                    tempMapPattrnInfo.mrightpoint = 35;
                    mmapPattrnInfo[index, 36].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 19;
                    mmapPattrnInfo[index, 37].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 25;
                    tempMapPattrnInfo.mrightpoint = 34;
                    mmapPattrnInfo[index, 37].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 9;
                    mmapPattrnInfo[index, 38].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 12;
                    tempMapPattrnInfo.mrightpoint = 33;
                    mmapPattrnInfo[index, 38].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 8;
                    mmapPattrnInfo[index, 39].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 14;
                    tempMapPattrnInfo.mrightpoint = 32;
                    mmapPattrnInfo[index, 39].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 8;
                    mmapPattrnInfo[index, 40].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 14;
                    tempMapPattrnInfo.mrightpoint = 32;
                    mmapPattrnInfo[index, 40].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 7;
                    mmapPattrnInfo[index, 41].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 15;
                    tempMapPattrnInfo.mrightpoint = 32;
                    mmapPattrnInfo[index, 41].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 18;
                    tempMapPattrnInfo.mrightpoint = 31;
                    mmapPattrnInfo[index, 42].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 21;
                    tempMapPattrnInfo.mrightpoint = 30;
                    mmapPattrnInfo[index, 43].Add(tempMapPattrnInfo);
                }

                //  Second Map Pattrn
                else if (index == 1)
                {
                    tempMapPattrnInfo.mleftPoint = 6;
                    tempMapPattrnInfo.mrightpoint = 8;
                    mmapPattrnInfo[index, 7].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 35;
                    tempMapPattrnInfo.mrightpoint = 36;
                    mmapPattrnInfo[index, 7].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 9;
                    mmapPattrnInfo[index, 8].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 19;
                    tempMapPattrnInfo.mrightpoint = 26;
                    mmapPattrnInfo[index, 8].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 33;
                    tempMapPattrnInfo.mrightpoint = 38;
                    mmapPattrnInfo[index, 8].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 10;
                    mmapPattrnInfo[index, 9].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 17;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 9].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 10;
                    mmapPattrnInfo[index, 10].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 15;
                    tempMapPattrnInfo.mrightpoint = 47;
                    mmapPattrnInfo[index, 10].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 11;
                    mmapPattrnInfo[index, 11].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 14;
                    tempMapPattrnInfo.mrightpoint = 47;
                    mmapPattrnInfo[index, 11].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 11;
                    mmapPattrnInfo[index, 12].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 15;
                    tempMapPattrnInfo.mrightpoint = 47;
                    mmapPattrnInfo[index, 12].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 12;
                    mmapPattrnInfo[index, 13].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 15;
                    tempMapPattrnInfo.mrightpoint = 47;
                    mmapPattrnInfo[index, 13].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 14].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 15].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 44;
                    mmapPattrnInfo[index, 16].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 42;
                    mmapPattrnInfo[index, 17].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 18;
                    mmapPattrnInfo[index, 18].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 28;
                    tempMapPattrnInfo.mrightpoint = 42;
                    mmapPattrnInfo[index, 18].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 16;
                    mmapPattrnInfo[index, 19].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 31;
                    tempMapPattrnInfo.mrightpoint = 42;
                    mmapPattrnInfo[index, 19].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 14;
                    mmapPattrnInfo[index, 20].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 32;
                    tempMapPattrnInfo.mrightpoint = 42;
                    mmapPattrnInfo[index, 20].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 2;
                    tempMapPattrnInfo.mrightpoint = 12;
                    mmapPattrnInfo[index, 21].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 33;
                    tempMapPattrnInfo.mrightpoint = 43;
                    mmapPattrnInfo[index, 21].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 12;
                    mmapPattrnInfo[index, 22].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 33;
                    tempMapPattrnInfo.mrightpoint = 43;
                    mmapPattrnInfo[index, 22].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 13;
                    mmapPattrnInfo[index, 23].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 33;
                    tempMapPattrnInfo.mrightpoint = 43;
                    mmapPattrnInfo[index, 23].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 14;
                    mmapPattrnInfo[index, 24].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 33;
                    tempMapPattrnInfo.mrightpoint = 42;
                    mmapPattrnInfo[index, 24].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 14;
                    mmapPattrnInfo[index, 25].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 29;
                    tempMapPattrnInfo.mrightpoint = 42;
                    mmapPattrnInfo[index, 25].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 15;
                    mmapPattrnInfo[index, 26].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 28;
                    tempMapPattrnInfo.mrightpoint = 41;
                    mmapPattrnInfo[index, 26].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 16;
                    mmapPattrnInfo[index, 27].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 26;
                    tempMapPattrnInfo.mrightpoint = 39;
                    mmapPattrnInfo[index, 27].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 18;
                    mmapPattrnInfo[index, 28].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 24;
                    tempMapPattrnInfo.mrightpoint = 39;
                    mmapPattrnInfo[index, 28].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 38;
                    mmapPattrnInfo[index, 29].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 38;
                    mmapPattrnInfo[index, 30].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 39;
                    mmapPattrnInfo[index, 31].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 39;
                    mmapPattrnInfo[index, 32].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 39;
                    mmapPattrnInfo[index, 33].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 41;
                    mmapPattrnInfo[index, 34].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 42;
                    mmapPattrnInfo[index, 35].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 13;
                    mmapPattrnInfo[index, 36].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 17;
                    tempMapPattrnInfo.mrightpoint = 42;
                    mmapPattrnInfo[index, 36].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 12;
                    mmapPattrnInfo[index, 37].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 18;
                    tempMapPattrnInfo.mrightpoint = 24;
                    mmapPattrnInfo[index, 37].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 27;
                    tempMapPattrnInfo.mrightpoint = 41;
                    mmapPattrnInfo[index, 37].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 12;
                    mmapPattrnInfo[index, 38].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 21;
                    tempMapPattrnInfo.mrightpoint = 23;
                    mmapPattrnInfo[index, 38].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 28;
                    tempMapPattrnInfo.mrightpoint = 35;
                    mmapPattrnInfo[index, 38].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 39;
                    tempMapPattrnInfo.mrightpoint = 40;
                    mmapPattrnInfo[index, 38].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 11;
                    mmapPattrnInfo[index, 39].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 28;
                    tempMapPattrnInfo.mrightpoint = 33;
                    mmapPattrnInfo[index, 39].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 6;
                    tempMapPattrnInfo.mrightpoint = 10;
                    mmapPattrnInfo[index, 40].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 29;
                    tempMapPattrnInfo.mrightpoint = 32;
                    mmapPattrnInfo[index, 40].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 9;
                    tempMapPattrnInfo.mrightpoint = 9;
                    mmapPattrnInfo[index, 41].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 29;
                    tempMapPattrnInfo.mrightpoint = 32;
                    mmapPattrnInfo[index, 41].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 30;
                    tempMapPattrnInfo.mrightpoint = 31;
                    mmapPattrnInfo[index, 42].Add(tempMapPattrnInfo);
                }

                //  Third Map Pattrn
                else if (index == 2)
                {
                    tempMapPattrnInfo.mleftPoint = 39;
                    tempMapPattrnInfo.mrightpoint = 42;
                    mmapPattrnInfo[index, 7].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 11;
                    tempMapPattrnInfo.mrightpoint = 12;
                    mmapPattrnInfo[index, 8].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 17;
                    tempMapPattrnInfo.mrightpoint = 27;
                    mmapPattrnInfo[index, 8].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 36;
                    tempMapPattrnInfo.mrightpoint = 43;
                    mmapPattrnInfo[index, 8].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 9;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 9].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 8;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 10].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 7;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 11].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 7;
                    tempMapPattrnInfo.mrightpoint = 31;
                    mmapPattrnInfo[index, 12].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 41;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 12].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 7;
                    tempMapPattrnInfo.mrightpoint = 26;
                    mmapPattrnInfo[index, 13].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 7;
                    tempMapPattrnInfo.mrightpoint = 26;
                    mmapPattrnInfo[index, 14].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 38;
                    tempMapPattrnInfo.mrightpoint = 40;
                    mmapPattrnInfo[index, 14].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 7;
                    tempMapPattrnInfo.mrightpoint = 27;
                    mmapPattrnInfo[index, 15].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 36;
                    tempMapPattrnInfo.mrightpoint = 43;
                    mmapPattrnInfo[index, 15].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 8;
                    tempMapPattrnInfo.mrightpoint = 28;
                    mmapPattrnInfo[index, 16].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 35;
                    tempMapPattrnInfo.mrightpoint = 44;
                    mmapPattrnInfo[index, 16].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 8;
                    tempMapPattrnInfo.mrightpoint = 28;
                    mmapPattrnInfo[index, 17].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 34;
                    tempMapPattrnInfo.mrightpoint = 44;
                    mmapPattrnInfo[index, 17].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 8;
                    tempMapPattrnInfo.mrightpoint = 27;
                    mmapPattrnInfo[index, 18].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 32;
                    tempMapPattrnInfo.mrightpoint = 44;
                    mmapPattrnInfo[index, 18].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 6;
                    tempMapPattrnInfo.mrightpoint = 26;
                    mmapPattrnInfo[index, 19].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 31;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 19].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 23;
                    mmapPattrnInfo[index, 20].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 30;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 20].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 22;
                    mmapPattrnInfo[index, 21].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 29;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 21].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 15;
                    mmapPattrnInfo[index, 22].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 29;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 22].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 13;
                    mmapPattrnInfo[index, 23].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 29;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 23].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 13;
                    mmapPattrnInfo[index, 24].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 29;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 24].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 6;
                    tempMapPattrnInfo.mrightpoint = 14;
                    mmapPattrnInfo[index, 25].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 30;
                    tempMapPattrnInfo.mrightpoint = 44;
                    mmapPattrnInfo[index, 25].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 6;
                    tempMapPattrnInfo.mrightpoint = 14;
                    mmapPattrnInfo[index, 26].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 29;
                    tempMapPattrnInfo.mrightpoint = 44;
                    mmapPattrnInfo[index, 26].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 6;
                    tempMapPattrnInfo.mrightpoint = 15;
                    mmapPattrnInfo[index, 27].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 29;
                    tempMapPattrnInfo.mrightpoint = 44;
                    mmapPattrnInfo[index, 27].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 6;
                    tempMapPattrnInfo.mrightpoint = 16;
                    mmapPattrnInfo[index, 28].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 27;
                    tempMapPattrnInfo.mrightpoint = 44;
                    mmapPattrnInfo[index, 28].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 6;
                    tempMapPattrnInfo.mrightpoint = 16;
                    mmapPattrnInfo[index, 29].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 26;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 29].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 17;
                    mmapPattrnInfo[index, 30].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 25;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 30].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 18;
                    mmapPattrnInfo[index, 31].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 24;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 31].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 18;
                    mmapPattrnInfo[index, 32].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 24;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 32].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 19;
                    mmapPattrnInfo[index, 33].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 24;
                    tempMapPattrnInfo.mrightpoint = 47;
                    mmapPattrnInfo[index, 33].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 19;
                    mmapPattrnInfo[index, 34].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 24;
                    tempMapPattrnInfo.mrightpoint = 47;
                    mmapPattrnInfo[index, 34].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 5;
                    tempMapPattrnInfo.mrightpoint = 19;
                    mmapPattrnInfo[index, 35].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 24;
                    tempMapPattrnInfo.mrightpoint = 46;
                    mmapPattrnInfo[index, 35].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 20;
                    mmapPattrnInfo[index, 36].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 24;
                    tempMapPattrnInfo.mrightpoint = 38;
                    mmapPattrnInfo[index, 36].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 43;
                    tempMapPattrnInfo.mrightpoint = 45;
                    mmapPattrnInfo[index, 36].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 20;
                    mmapPattrnInfo[index, 37].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 24;
                    tempMapPattrnInfo.mrightpoint = 38;
                    mmapPattrnInfo[index, 37].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 20;
                    mmapPattrnInfo[index, 38].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 24;
                    tempMapPattrnInfo.mrightpoint = 37;
                    mmapPattrnInfo[index, 38].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 13;
                    mmapPattrnInfo[index, 39].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 18;
                    tempMapPattrnInfo.mrightpoint = 19;
                    mmapPattrnInfo[index, 39].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 23;
                    tempMapPattrnInfo.mrightpoint = 37;
                    mmapPattrnInfo[index, 39].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 13;
                    mmapPattrnInfo[index, 40].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 22;
                    tempMapPattrnInfo.mrightpoint = 38;
                    mmapPattrnInfo[index, 40].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 4;
                    tempMapPattrnInfo.mrightpoint = 12;
                    mmapPattrnInfo[index, 41].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 23;
                    tempMapPattrnInfo.mrightpoint = 38;
                    mmapPattrnInfo[index, 41].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 8;
                    tempMapPattrnInfo.mrightpoint = 11;
                    mmapPattrnInfo[index, 42].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 36;
                    tempMapPattrnInfo.mrightpoint = 37;
                    mmapPattrnInfo[index, 42].Add(tempMapPattrnInfo);
                }

                //  Fourth Map Pattrn
                else if (index == 3)
                {
                    tempMapPattrnInfo.mleftPoint = 6;
                    tempMapPattrnInfo.mrightpoint = 8;
                    mmapPattrnInfo[index, 8].Add(tempMapPattrnInfo);
                    tempMapPattrnInfo.mleftPoint = 33;
                    tempMapPattrnInfo.mrightpoint = 37;
                    mmapPattrnInfo[index, 8].Add(tempMapPattrnInfo);

                    tempMapPattrnInfo.mleftPoint = 3;
                    tempMapPattrnInfo.mrightpoint = 2;
                    mmapPattrnInfo[index, 9].Add(tempMapPattrnInfo);
                }

                //  Fifth Map Pattrn
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //  "mapIndex" and "SetMapPattenInfo()" are incomplete parts. Please note that my GitHub contains information on drawing pictures using Fourier transform as a solution to supplement this part. 
            NodeManager nodeManager = new NodeManager(50, 50, 75, 45, 33, 100, 2);
            nodeManager.DrawMap();
        }
    }
}

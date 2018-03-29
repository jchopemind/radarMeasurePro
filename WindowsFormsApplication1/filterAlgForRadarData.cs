using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class filterAlgForRadarData
    {
        static public int startAngle_int = -1;
        static public int allAngle_size = -1;

        static private List<float> list_distance = new List<float>();
        static private List<int> list_angle = new List<int>();

        static public List<float> list_final_points = new List<float>();
        static public List<int> list_final_points_angle = new List<int>();

        static public List<float> list_final_points_end = new List<float>();
        static public List<float> list_final_points_angle_last_end = new List<float>();
        static public List<float> list_final_pionts_curlcz_end = new List<float>();

        static public List<int> list_final_points_angle_end = new List<int>();

        static private int isStart = 0;
        static public int minmin1_sc = -1;
        static public int minmin2_sc = -1;
        static private int alpha1 = -1;
        static private int alpha2 = -1;

        static public float lower_limit = 2000.00f;
        static public float super_limit = 12000.00f;

        static private int alg_status = 1;

        /* for 4 points, last 2 point */
        static private float last_point1 = 0.0f;
        static private float last_point2 = 0.0f;


        static public AllParameter parameter = new AllParameter();

        static private void filterDataInit()
        {
            list_final_points.Clear();
            list_final_points_angle.Clear();
            list_distance.Clear();
            list_angle.Clear();
            list_final_points_end.Clear();
            list_final_points_angle_end.Clear();
            list_final_pionts_curlcz_end.Clear();
            isStart = 0;
        }
        //parameter.daogao = parameter.LMSxx_Y1 + parameter.LMSxx_Y2 + minPoint * (float)Math.Sin(angel * parameter.OneDegree);//导高
        //parameter.daogao = (float)Math.Round(parameter.daogao, 2);//保留2位小数




        #region for 导高
        private float lastX = 5900;
        private float lastP = 20;
        public float kalmanFilter(float meVal)
        {
            float curX = lastX;
            float curP = lastP + 0.05f;
            float kg = curP / (curP + 0.5f);
            float Bs = curX + kg * (meVal - curX);
            lastP = curP - kg * curP;
            lastX = Bs;

            return Bs;
        }
        #endregion


        static public void filterRadarData(String[] sArray, float angle_resolution)
        {
            List<float> list_distance = new List<float>();
            List<int> list_angle = new List<int>();

            filterDataInit();

            //Console.Write("datastart: ");
            int starti = 26 + startAngle_int * 6;
            int dataCounts = starti + allAngle_size * 6;

            if (alg_status == 1)
            {
                int dataNums = Convert.ToInt32(sArray[25], 16);
                //Console.WriteLine("dataNums = " + dataNums);
               
                last_point1 = 0.0f;
                last_point2 = 0.0f;

                for (int i = starti; i < dataCounts; i++)
                {
                    int realVV = Convert.ToInt32(sArray[i], 16);

                    /* filter for data set */
                    float realV = parameter.LMSxx_Y1 + parameter.LMSxx_Y2 + realVV * (float)Math.Sin((i - 26) * angle_resolution * parameter.OneDegree);
                    //Console.Write(realV.ToString() + ", ");

                    if (realV < lower_limit || realV > super_limit || (i == (26 + dataNums - 1)))
                    {
                        if (list_distance.Count >= 1 && list_angle.Count >= 1 && isStart == 1)
                        {
                            for (int ii = 0; ii < list_distance.Count; ii++)
                            {
                                if (list_distance.Min() == list_distance[ii])
                                {
                                    list_final_points.Add(list_distance[ii]);
                                    list_final_points_angle.Add(list_angle[ii]);
                                }
                            }
                            list_distance.Clear();
                            list_angle.Clear();
                            isStart = 0;
                        }

                        list_distance.Clear();
                        list_angle.Clear();
                        isStart = 0;
                    }
                    else
                    {
                        if (isStart == 1)
                        {
                            /* For more details, must be fixed this parameter */
                            if (Math.Abs(list_distance.Last<float>() - realV) <= 50)
                            {
                                list_distance.Add(realV);
                                list_angle.Add(i - 26);
                            }
                            else
                            {
                                if (list_distance.Count >= 1 && list_angle.Count >= 1 && isStart == 1)
                                {
                                    for (int ii = 0; ii < list_distance.Count; ii++)
                                    {
                                        if (list_distance.Min() == list_distance[ii])
                                        {
                                            list_final_points.Add(list_distance[ii]);
                                            list_final_points_angle.Add(list_angle[ii]);
                                        }
                                    }
                                }

                                list_distance.Clear();
                                list_angle.Clear();
                                isStart = 0;
                            }
                        }
                        else
                        {
                            list_distance.Add(realV);
                            list_angle.Add(i - 26);
                            isStart = 1;
                        }
                    }
                }

                /* Remove duplicate elements */
                if (list_final_points.Count > 1)
                {
                    for (int kk = 0; kk < list_final_points.Count; kk++)
                    {
                        for (int ll = list_final_points.Count - 1; ll > kk; ll--)
                        {
                            if ((Math.Abs(list_final_points[kk] - list_final_points[ll]) < 3))// || (Math.Abs(list_final_points_angle[kk] - list_final_points_angle[ll]) < 3))
                            {
                                list_final_points.RemoveAt(ll);
                                list_final_points_angle.RemoveAt(ll);
                            }
                        }
                    }
                }

               //Console.WriteLine();
                //Console.WriteLine("list_final_points: " + list_final_points.Count.ToString());

                if (list_final_points.Count == 2)
                {
                    alg_status = 1;
                    minmin1_sc = list_final_points.IndexOf(list_final_points.Min());
                    alpha1 = list_final_points_angle[minmin1_sc];
                    list_final_points_end.Add(list_final_points[minmin1_sc]);
                    list_final_points_angle_end.Add(list_final_points_angle[minmin1_sc]);
                }
                else if (list_final_points.Count == 4)
                {
                    List<float> tep_list = new List<float>(list_final_points);

                    alg_status = 2;

                    tep_list.RemoveAt(tep_list.IndexOf(tep_list.Min()));
                    minmin1_sc = list_final_points.IndexOf(list_final_points.Min());
                    minmin2_sc = list_final_points.IndexOf(tep_list.Min());
                    alpha1 = list_final_points_angle[minmin1_sc];
                    alpha2 = list_final_points_angle[minmin2_sc];

                    list_final_points_end.Add(list_final_points[minmin1_sc]);
                    list_final_points_angle_end.Add(list_final_points_angle[minmin1_sc]);
                    list_final_points_end.Add(list_final_points[minmin2_sc]);
                    list_final_points_angle_end.Add(list_final_points_angle[minmin2_sc]);

                    //Console.WriteLine("end points : " + list_final_points_end.Count);

                    Console.WriteLine("第1个点角度：" + list_final_points_angle_end[0]);
                    Console.WriteLine("第2个点角度：" + list_final_points_angle_end[1]);


                }
                else if (list_final_points.Count == 0 && list_final_points.Count == 0)
                {
                    alg_status = 1;
                }
                else
                {
                    minmin1_sc = -1;
                    minmin2_sc = -1;
                    alpha2 = alpha1 = -1;

                    list_final_points_end.Add(list_final_points.Min());
                    list_final_points_angle_end.Add(list_final_points_angle[list_final_points.IndexOf(list_final_points.Min())]);
                }
            }
            else if (alg_status == 2)
            {
                if (Math.Abs(alpha2 - alpha1) > 5)
                {
                    int mid_angle = (alpha2 + alpha1) / 2;
                    float minPoint = 99999.0f;
                    int angel_flag = -1;

                    for (int i = starti; i < 26 + mid_angle; i++)
                    {
                        int realVV = Convert.ToInt32(sArray[i], 16);
                        float realV = parameter.LMSxx_Y1 + parameter.LMSxx_Y2 + realVV * (float)Math.Sin((i - 26) * angle_resolution * parameter.OneDegree);


                         if (realV > super_limit || realV < lower_limit)
                            continue;

                        if (realV < minPoint)
                        {
                            minPoint = realV;
                            angel_flag = i - 26;
                            alpha1 = angel_flag;
                        }
                    }

                    if (minPoint == 99999 || minPoint <= lower_limit)
                    {
                        alg_status = 1;
                    }
                    else
                    {
                        if (last_point1 == 0.0f)
                        {
                            last_point1 = minPoint;
                            list_final_points_end.Add(minPoint);
                            list_final_points_angle_end.Add(alpha1);
                        }
                        else if (Math.Abs(last_point1 - minPoint) > 200)
                        {
                            alg_status = 1;
                        }else
                        {
                            list_final_points_end.Add(minPoint);
                            list_final_points_angle_end.Add(alpha1);
                        }

                    }

                    minPoint = 99999.0f;
                    for (int i = 26 + mid_angle; i < dataCounts; i++)
                    {
                        int realVV = Convert.ToInt32(sArray[i], 16);
                        float realV = parameter.LMSxx_Y1 + parameter.LMSxx_Y2 + realVV * (float)Math.Sin((i - 26) * angle_resolution * parameter.OneDegree);

                        if (realV > super_limit || realV < lower_limit)
                            continue;

                        if (realV < minPoint)
                        {
                            minPoint = realV;
                            angel_flag = i - 26;
                            alpha2 = angel_flag;
                        }
                    }

                    if (minPoint == 99999 && minPoint <= lower_limit)
                    {
                        alg_status = 1;
                    }
                    else
                    {
                        if (last_point2 == 0)
                        {
                            last_point2 = minPoint;
                            list_final_points_end.Add(minPoint);
                            list_final_points_angle_end.Add(alpha2);
                        }
                        else if (Math.Abs(last_point2 - minPoint) > 1000)
                        {
                            alg_status = 1;
                        } else {
                            list_final_points_end.Add(minPoint);
                            list_final_points_angle_end.Add(alpha2);
                        }
                    }

                }
                else
                {
                    //float daogao_i;
                    float minPoint = 99999.0f;
                    int angel_flag = -1;

                    alg_status = 1;

                    for (int i = starti; i < dataCounts; i++)
                    {
                        int realVV = Convert.ToInt32(sArray[i], 16);
                        float realV = parameter.LMSxx_Y1 + parameter.LMSxx_Y2 + realVV * (float)Math.Sin((i - 26) * angle_resolution * parameter.OneDegree);

                        if (realV > super_limit ||  realV < lower_limit)
                            continue;

                        //daogao_i = parameter.LMSxx_Y1 + parameter.LMSxx_Y2 + realV * (float)Math.Sin(angel * parameter.OneDegree);//导高
                        /* filter for data set */

                        if (realV < minPoint)
                        {
                            minPoint = realV;
                            angel_flag = i - 26;
                            alpha1 = angel_flag;
                            alpha2 = -1;
                        }
                    }

                    list_final_points_end.Add(minPoint);
                    list_final_points_angle_end.Add(angel_flag);
                }
            }

            if (filterAlgForRadarData.list_final_points_end.Count == 2)
            {
                if (Math.Abs(filterAlgForRadarData.list_final_points_end[0] - filterAlgForRadarData.list_final_points_end[1]) > 800)
                {
                    filterAlgForRadarData.list_final_points_angle_end.Remove(filterAlgForRadarData.list_final_points_end.IndexOf(filterAlgForRadarData.list_final_points_end.Max()));
                    filterAlgForRadarData.list_final_points_end.Remove(filterAlgForRadarData.list_final_points_end.Max());
                }
            }

            for (int i = 0; i < list_final_points_end.Count; i++)
            {
                float angle = (float)list_final_points_angle_end[i] * angle_resolution * (float)parameter.OneDegree;
                float curRadarValue = list_final_points_end[i];
                list_final_pionts_curlcz_end.Add(
                parameter.LMSxx_X1 + parameter.LMSxx_X2 + curRadarValue * (float)Math.Cos(angle) - 1435 / 2);
            }

            Console.WriteLine("CurPoints: " + list_final_points.Count);
        }

        static public List<int> list_cmxj_end = new List<int>();


        static public int getCMXJ_DATA(String[] sArray, float angle)
        {
            int ldata1 = Convert.ToInt32(sArray[26], 16);
            int ldata2 = Convert.ToInt32(sArray[27], 16);
            int ldata3 = Convert.ToInt32(sArray[28], 16);
            int rdata1 = Convert.ToInt32(sArray[26 + 1080], 16);
            int rdata2 = Convert.ToInt32(sArray[26 + 1079], 16);
            int rdata3 = Convert.ToInt32(sArray[26 + 1078], 16);

            list_cmxj_end.Clear();

            if(Math.Abs(ldata1 - ldata2) < 50)
            {
                if (Math.Abs(ldata2 - ldata3) < 50)
                {
                    int averagel = (ldata1 + ldata2 + ldata3) / 3;
                    int cmxj = Convert.ToInt32(averagel * Math.Cos(angle * 0.0174533));
                    if(cmxj >= 1200 && cmxj <= 2500)
                    {
                        list_cmxj_end.Add(cmxj);
                    }
                }
            }

            if (Math.Abs(rdata1 - rdata2) < 50)
            {
                if (Math.Abs(rdata2 - rdata3) < 50)
                {
                    int averager = (rdata1 + rdata2 + rdata3) / 3;
                    int cmxj = Convert.ToInt32(averager * Math.Cos(angle * 0.0174533));

                    if (cmxj >= 1200 && cmxj <= 2500)
                    {
                        list_cmxj_end.Add(cmxj);
                    }
                }
            }

            if(list_cmxj_end.Count == 1)
            {
                return list_cmxj_end[0];
            } else {
                //Console.WriteLine("侧面限界1：" + list_cmxj_end[0] + "; 侧面限界2：" + list_cmxj_end[1]);
                return 0;
            }
        }
    }
}

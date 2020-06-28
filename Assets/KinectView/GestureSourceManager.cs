using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using UnityEngine.UI;
using System.Linq;

// Adapted from DiscreteGestureBasics-WPF by Momo the Monster 2014-11-25
// For Helios Interactive - http://heliosinteractive.com

public class GestureSourceManager : MonoBehaviour
{

    public struct EventArgs
    {
        public string name;
        public float confidence;

        public EventArgs(string _name, float _confidence)
        {
            name = _name;
            confidence = _confidence;
        }
    }

    public BodySourceManager _BodySource;
    public string databasePath = "Gestures.gbd";
    public Text Result;

    // 姿势名称列表
    public static string[] GestureNames =
    new string[]{
        "ArmExtend",    //双手前平举
        "Bobath",
        "FeetTogetherStand",    //双足并拢站立
        "LeftLegStand", //左脚单脚站立
        "RightLegStand",    //右脚单脚站立
        "Sit",  //坐
        "Stand",    //站
        "Sit2Stand",    //由坐到站
        "Stand2Sit" //由站到坐
    };

    // 制定姿势优先级，越大则越优先
    public static Dictionary<string, int> Priority =
    new Dictionary<string, int>{
        { "ArmExtend", 1 },
        {"Bobath", 1 },
        {"FeetTogetherStand", 1 },
        {"LeftLegStand", 2 },
        {"RightLegStand", 2 },
        {"Sit", 1 },
        {"Stand", 0 },
        {"Sit2Stand", 1 },
        {"Stand2Sit", 1 }
    };


    private KinectSensor _Sensor;
    private VisualGestureBuilderFrameSource _Source;
    private VisualGestureBuilderFrameReader _Reader;
    private VisualGestureBuilderDatabase _Database;

    // Gesture Detection Events
    public delegate void GestureAction(EventArgs e);
    public event GestureAction OnGesture;


    private static GestureSourceManager instance = null;
    public static GestureSourceManager Instance { 
        get
        {
            return instance;
        }
    }



    // Use this for initialization
    void Start()
    {
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor != null)
        {

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }

            // Set up Gesture Source
            _Source = VisualGestureBuilderFrameSource.Create(_Sensor, 0);

            // open the reader for the vgb frames
            _Reader = _Source.OpenReader();
            if (_Reader != null)
            {
                _Reader.IsPaused = true;
                _Reader.FrameArrived += GestureFrameArrived;    //此处为逐帧调用可能卡
            }

            // load the 'Seated' gesture from the gesture database
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, databasePath);
            Debug.Log("Database path is: " + path);
            _Database = VisualGestureBuilderDatabase.Create(path);

            // Load all gestures
            IList<Gesture> gesturesList = _Database.AvailableGestures;
            for (int g = 0; g < gesturesList.Count; g++)
            {
                Gesture gesture = gesturesList[g];
                Debug.Log("Database name is: " + gesture.Name);
                _Source.AddGesture(gesture);
            }

        }

        OnGesture += ShowInText;
        instance = this;
    }

    // Public setter for Body ID to track
    public void SetBody(ulong id)
    {
        if (id > 0)
        {
            _Source.TrackingId = id;
            _Reader.IsPaused = false;
        }
        else
        {
            _Source.TrackingId = 0;
            _Reader.IsPaused = true;
        }
    }

    // Update Loop, set body if we need one
    void Update()
    {
        if (!_Source.IsTrackingIdValid)
        {
            FindValidBody();
        }
    }

    // Check Body Manager, grab first valid body
    void FindValidBody()
    {

        if (_BodySource != null)
        {
            Body[] bodies = _BodySource.GetData();
            if (bodies != null)
            {
                foreach (Body body in bodies)
                {
                    if (body.IsTracked)
                    {
                        SetBody(body.TrackingId);
                        break;
                    }
                }
            }
        }

    }

    /// Handles gesture detection results arriving from the sensor for the associated body tracking Id
    private void GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
    {
        VisualGestureBuilderFrameReference frameReference = e.FrameReference;
        using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
        {
            if (frame != null)
            {
                // get the discrete gesture results which arrived with the latest frame
                IDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;

                if (discreteResults != null)
                {

                    List<EventArgs> Results = new List<EventArgs>();

                    foreach (Gesture gesture in _Source.Gestures)
                    {
                        //Debug.Log(gesture.Name);
                        //Debug.Log(gesture.GestureType);
                        if (gesture.GestureType == GestureType.Discrete)    //离散型
                        {
                            DiscreteGestureResult result = null;
                            discreteResults.TryGetValue(gesture, out result);

                            if (result != null)
                            {
                                if(IsDetected(Priority[gesture.Name], result.Confidence))    //该动作是否完成
                                {
                                    Results.Add(new EventArgs(gesture.Name, result.Confidence));    //完成则加入判断列表

                                    //Debug.Log("Detected Gesture " + gesture.Name + " with Confidence " + result.Confidence);
                                    // Fire Event
                                    //OnGesture(new EventArgs(gesture.Name, result.Confidence));
                                    //return;
                                }
 
                            }
                        }
                    }

                    EventArgs Result = GestureJudgement(Results);
                    Debug.Log("Detected Gesture " + Result.name + " with Confidence " + Result.confidence.ToString());
                    OnGesture(Result);
                }
            }

            //OnGesture(new EventArgs("NO-Gesture", 0));
        }
    }

    private void ShowInText(EventArgs e)
    {
        this.Result.text = "Detected Gesture " + e.name + " with Confidence " + e.confidence;
    }


    /// <summary>
    /// 由Rank和Confidence判断该动作是否可能被识别
    /// </summary>
    /// <param name="Rank"></param>
    /// <param name="Confidence"></param>
    /// <returns></returns>
    private bool IsDetected(int Rank, float Confidence)
    {
        switch (Rank)
        {
            case 2:
                return Confidence > 0.5;
            case 1:
                return Confidence > 0.7;
            case 0:
                return Confidence > 0.95;
            default:
                return Confidence > 0.95;
        }
    }

    /// <summary>
    /// 从Results中判断当前最符合哪个姿势
    /// </summary>
    /// <param name="Results"></param>
    /// <returns></returns>
    private EventArgs GestureJudgement(List<EventArgs> Results)
    {
        List<EventArgs> Rank2 = new List<EventArgs>();
        List<EventArgs> Rank1 = new List<EventArgs>();
        List<EventArgs> Rank0 = new List<EventArgs>();

        foreach (EventArgs result in Results)
        {
            switch (Priority[result.name])
            {
                case 2:
                        Rank2.Add(result);
                    break;
                case 1:
                        Rank1.Add(result);
                    break;
                case 0:
                        Rank0.Add(result);
                    break;
            }
        }

        if (Rank2.Count != 0)
        {
            return SelectGesture(Rank2);
        }
        else if (Rank1.Count != 0)
        {
            return SelectGesture(Rank1);
        }
        else if(Rank0.Count != 0)
        {
            return SelectGesture(Rank0);
        }
        else
        {
            return new EventArgs("NO-Gesture", 0);
        }

    }

    /// <summary>
    /// 返回最大confidence对应的元素
    /// </summary>
    /// <param name="Rank">输入的EventArgs列表</param>
    /// <returns></returns>
    private EventArgs SelectGesture(List<EventArgs> Rank)
    {
        if (Rank.Count == 0)
        {
            throw new System.Exception("List<EventArgs> Rank is void.");
        }
        Rank.Sort(new GestureComparer());
        return Rank.Last<EventArgs>();
    }



    
    /// <summary>
    /// 自定义比较器
    /// </summary>
    class GestureComparer : IComparer<EventArgs>
    {
        public int Compare(EventArgs x, EventArgs y)
        {
            if(x.confidence > y.confidence)
            {
                return 1;
            }
            else if(Mathf.Approximately(x.confidence, y.confidence)){
                return 0;
            }
            else
            {
                return -1;
            }
        }


    }
}
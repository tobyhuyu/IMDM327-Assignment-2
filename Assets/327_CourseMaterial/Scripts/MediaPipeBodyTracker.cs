using System.Collections;
using Mediapipe;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample.Holistic;
using UnityEngine;

[DisallowMultipleComponent]
public class MediaPipeBodyTracker : MonoBehaviour
{
    [Header("Graph Source")]
    [SerializeField] private HolisticTrackingGraph graphRunner;

    [Header("Hand Data")]
    [SerializeField] private Vector3 leftHandPosition;
    [SerializeField] private bool leftHandPinch;

    [SerializeField] private bool leftHandScissors;
    [SerializeField] private bool leftHandRock;
    [SerializeField] private bool leftHandPaper;

    [SerializeField] private Vector3 rightHandPosition;
    [SerializeField] private bool rightHandPinch;


    [SerializeField] private bool rightHandScissors;
    [SerializeField] private bool rightHandRock;
    [SerializeField] private bool rightHandPaper;



    [Header("Debug / Diagnostics")]
    [SerializeField] private bool verboseLogging = false;
    [SerializeField] private int leftHandPacketCount;
    [SerializeField] private int rightHandPacketCount;
    [SerializeField] private int posePacketCount;

    [Header("Detection Settings")]
    [SerializeField, Range(0.001f, 0.2f)] private float pinchThreshold = 0.035f;
    [SerializeField, Range(0.001f, 0.2f)] private float rockThreshold = 0.035f;
    [SerializeField, Range(0.001f, 0.2f)] private float paperThreshold = 0.5f;
    [SerializeField, Range(0.001f, 0.2f)] private float scissorsThreshold = 0.3f;

    private readonly object dataLock = new object();
    private Vector3 pendingLeftHandPosition;
    private Vector3 pendingRightHandPosition;
 
    private bool pendingLeftPinch;

    private bool pendingLeftScissors;
    private bool pendingLeftRock;
    private bool pendingLeftPaper;

    private bool pendingRightPinch;

    private bool pendingRightScissors;
    private bool pendingRightRock;
    private bool pendingRightPaper;

    private bool leftHandTracked;
    private bool rightHandTracked;
    private bool poseTracked;
    private bool leftDirty;
    private bool rightDirty;
    private bool poseDirty;
    private Coroutine subscribeRoutine;
    private bool hasSubscriptions;
    private HolisticTrackingGraph subscribedRunner;

    public Vector3 LeftHandPosition => leftHandPosition;
    public bool LeftHandPinch => leftHandPinch;

    public bool LeftHandScissors => leftHandScissors;
    public bool LeftHandRock => leftHandRock;
    public bool LeftHandPaper => leftHandPaper;

    public bool LeftHandTracked => isActiveAndEnabled && leftHandTracked;
    public Vector3 RightHandPosition => rightHandPosition;
    public bool RightHandPinch => rightHandPinch;

    public bool RightHandScissors => rightHandScissors;
    public bool RightHandRock => rightHandRock;
    public bool RightHandPaper => rightHandPaper;

    public bool RightHandTracked => isActiveAndEnabled && rightHandTracked;

    private bool summonRock = false;
    private bool summonPaper = false;
    private bool summonScissors = false;

    private Summoner summoner;
  

    private void OnEnable()
    {
        summoner = GetComponent<Summoner>();

        if (graphRunner == null)
        {
            graphRunner = FindObjectOfType<HolisticTrackingGraph>();
        }

        if (graphRunner == null)
        {
            Debug.LogWarning($"{nameof(MediaPipeBodyTracker)} on {name} could not find a {nameof(HolisticTrackingGraph)}.");
            return;
        }

        if (!TrySubscribe())
        {
            if (verboseLogging)
            {
                Debug.Log($"{nameof(MediaPipeBodyTracker)} waiting for graph runner to initialise...");
            }
            subscribeRoutine = StartCoroutine(SubscribeWhenReady());
        }
    }

    private void OnDisable()
    {
        if (subscribeRoutine != null)
        {
            StopCoroutine(subscribeRoutine);
            subscribeRoutine = null;
        }

        Unsubscribe();
        lock (dataLock)
        {
            leftHandTracked = rightHandTracked = poseTracked = false;
            leftHandPinch = rightHandPinch = false;
            leftHandScissors = rightHandScissors = leftHandRock = rightHandRock = leftHandPaper = rightHandPaper = false;
            leftHandPosition = rightHandPosition = Vector3.zero;
        }
    }

    public void StopTracking()
    {
        OnDisable();
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (isActiveAndEnabled)
        {
            if (TrySubscribe())
            {
                subscribeRoutine = null;
                yield break;
            }

            yield return null;
        }
    }

    private bool TrySubscribe()
    {
        if (graphRunner == null || hasSubscriptions)
        {
            return hasSubscriptions;
        }

        try
        {
            graphRunner.OnLeftHandLandmarksOutput += HandleLeftHand;
            graphRunner.OnRightHandLandmarksOutput += HandleRightHand;
            subscribedRunner = graphRunner;
            hasSubscriptions = true;

            if (verboseLogging)
            {
                Debug.Log($"{nameof(MediaPipeBodyTracker)} listening to {graphRunner.name}");
            }

            return true;
        }
        catch (System.NullReferenceException)
        {
            return false;
        }
    }

    private void Unsubscribe()
    {
        if (!hasSubscriptions)
        {
            subscribedRunner = null;
            hasSubscriptions = false;
            return;
        }

        var runner = subscribedRunner != null ? subscribedRunner : graphRunner;
        if (runner != null)
        {
            try
            {
                runner.OnLeftHandLandmarksOutput -= HandleLeftHand;
                runner.OnRightHandLandmarksOutput -= HandleRightHand;
            }
            catch (System.NullReferenceException) { }
        }

        subscribedRunner = null;
        hasSubscriptions = false;
    }

    private void Update()
    {
        lock (dataLock)
        {
            if (leftDirty)
            {
                if (leftHandTracked)
                {
                    leftHandPosition = pendingLeftHandPosition;
                    leftHandPinch = pendingLeftPinch;

                    leftHandScissors = pendingLeftScissors;
                    leftHandRock = pendingLeftRock;
                    leftHandPaper = pendingLeftPaper;
                }
                else
                {
                    leftHandPosition = Vector3.zero;
                    leftHandPinch = false;

                    leftHandScissors = leftHandRock = leftHandPaper = false;
                }

                leftDirty = false;
            }

            if (rightDirty)
            {
                if (rightHandTracked)
                {
                    rightHandPosition = pendingRightHandPosition;
                    rightHandPinch = pendingRightPinch;

                    rightHandScissors = pendingRightScissors;
                    rightHandRock = pendingRightRock;
                    rightHandPaper = pendingRightPaper;
                }
                else
                {
                    rightHandPosition = Vector3.zero;
                    rightHandPinch = false;

                    rightHandScissors = rightHandRock = rightHandPaper = false;
                }

                rightDirty = false;
            }
        }

        if (summonRock){
            summoner.Create(1,1);
            summonRock = false;
        }

        if (summonScissors)
        {
            summoner.Create(1,3);
            summonScissors = false;
        }
        
        if (summonPaper)
        {
            summoner.Create(1,2);
            summonPaper = false;
        }

        
    }

    private void HandleLeftHand(object sender, OutputStream<NormalizedLandmarkList>.OutputEventArgs eventArgs)
    {
        leftHandPacketCount++;
        if (verboseLogging && (leftHandPacketCount % 30 == 0))
        {
            Debug.Log($"{nameof(MediaPipeBodyTracker)} received {leftHandPacketCount} left-hand packets");
        }

        var landmarkList = ExtractNormalizedLandmarks(eventArgs.packet);

        lock (dataLock)
        {
            if (landmarkList != null && landmarkList.Landmark.Count > 0)
            {
                leftHandTracked = true;
                pendingLeftHandPosition = ToUnityVector(landmarkList.Landmark[0]);
                pendingLeftPinch = IsPinching(landmarkList);

                pendingLeftScissors = IsScissors(landmarkList);
                pendingLeftRock = IsRock(landmarkList);
                pendingLeftPaper = IsPaper(landmarkList);
            }
            else
            {
                leftHandTracked = false;
                pendingLeftHandPosition = Vector3.zero;
                pendingLeftPinch = false;
                pendingLeftScissors = pendingLeftRock = pendingLeftPaper = false;
            }

            leftDirty = true;
        }
    }

    private void HandleRightHand(object sender, OutputStream<NormalizedLandmarkList>.OutputEventArgs eventArgs)
    {
        rightHandPacketCount++;
        if (verboseLogging && (rightHandPacketCount % 30 == 0))
        {
            Debug.Log($"{nameof(MediaPipeBodyTracker)} received {rightHandPacketCount} right-hand packets");
        }

        var landmarkList = ExtractNormalizedLandmarks(eventArgs.packet);

        lock (dataLock)
        {
            if (landmarkList != null && landmarkList.Landmark.Count > 0)
            {
                rightHandTracked = true;
                pendingRightHandPosition = ToUnityVector(landmarkList.Landmark[0]);
                pendingRightPinch = IsPinching(landmarkList);

                pendingRightScissors = IsScissors(landmarkList);
                pendingRightRock = IsRock(landmarkList);
                pendingRightPaper = IsPaper(landmarkList);
            }
            else
            {
                rightHandTracked = false;
                pendingRightHandPosition = Vector3.zero;
                pendingRightPinch = false;

                pendingRightScissors = pendingRightRock = pendingRightPaper = false;
            }

            rightDirty = true;
        }
    }

    private NormalizedLandmarkList ExtractNormalizedLandmarks(Packet<NormalizedLandmarkList> packet)
    {
        return packet == null ? null : packet.Get(NormalizedLandmarkList.Parser);
    }

    private LandmarkList ExtractWorldLandmarks(Packet<LandmarkList> packet)
    {
        return packet == null ? null : packet.Get(LandmarkList.Parser);
    }

    private Vector3 ToUnityVector(NormalizedLandmark landmark)
    {
        return new Vector3(landmark.X, landmark.Y, landmark.Z);
    }

    private Vector3 ToUnityVector(Landmark landmark)
    {
        return new Vector3(landmark.X, landmark.Y, landmark.Z);
    }

    private bool IsPinching(NormalizedLandmarkList landmarks)
    {
        if (landmarks.Landmark.Count <= 8)
        {
            return false;
        }

        var thumbTip = landmarks.Landmark[4];
        var indexTip = landmarks.Landmark[8];
        var thumb = new Vector2(thumbTip.X, thumbTip.Y);
        var index = new Vector2(indexTip.X, indexTip.Y);
        var distance = Vector2.Distance(thumb, index);

        if (distance > pinchThreshold)
        {
            return false;
        }

        if (landmarks.Landmark.Count > 12)
        {
            var middleTip = landmarks.Landmark[12];
            var middle = new Vector2(middleTip.X, middleTip.Y);
            var indexToMiddle = Vector2.Distance(index, middle);
            if (distance > indexToMiddle)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsScissors(NormalizedLandmarkList landmarks)
    {
        var thumbL = landmarks.Landmark[4];
        var finger1Tip = landmarks.Landmark[8];
        var finger2Tip = landmarks.Landmark[12];
        var finger3Tip = landmarks.Landmark[16];
        var finger4Tip = landmarks.Landmark[20];
        var wristL = landmarks.Landmark[0];

        var thumb = new Vector2(thumbL.X, thumbL.Y);
        var finger1 = new Vector2(finger1Tip.X, finger1Tip.Y);
        var finger2 = new Vector2(finger2Tip.X, finger2Tip.Y);
        var finger3 = new Vector2(finger3Tip.X, finger3Tip.Y);
        var finger4 = new Vector2(finger4Tip.X, finger4Tip.Y);
        var wrist = new Vector2(wristL.X, wristL.Y);

        // Debug.Log("distances " + Vector2.Distance(finger1,wrist) + " " + Vector2.Distance(finger2,wrist) + " " 
        // + Vector2.Distance(finger3,wrist) + " " + Vector2.Distance(finger4,wrist));
        
        //check thumb, pinky and ring finger distance to palm (close) 
        //and check pointer and middle finger to palm (far)
        if((Vector2.Distance(thumb, wrist) > scissorsThreshold || Vector2.Distance(finger3, wrist) > scissorsThreshold
        || Vector2.Distance(finger4, wrist) > scissorsThreshold) || (Vector2.Distance(finger1, wrist) < paperThreshold
        || Vector2.Distance(finger2, wrist) < paperThreshold))
        {
            return false;
        }

        summonScissors = true;
        return true;
    }

    private bool IsRock(NormalizedLandmarkList landmarks)
    {
        var finger1Tip = landmarks.Landmark[8];
        var finger2Tip = landmarks.Landmark[12];
        var finger3Tip = landmarks.Landmark[16];
        var finger4Tip = landmarks.Landmark[20];

        var finger1bTip = landmarks.Landmark[5];
        var finger2bTip = landmarks.Landmark[9];
        var finger3bTip = landmarks.Landmark[13];
        var finger4bTip = landmarks.Landmark[17];

        var finger1 = new Vector2(finger1Tip.X, finger1Tip.Y);
        var finger2 = new Vector2(finger2Tip.X, finger2Tip.Y);
        var finger3 = new Vector2(finger3Tip.X, finger3Tip.Y);
        var finger4 = new Vector2(finger4Tip.X, finger4Tip.Y);

        var finger1b = new Vector2(finger1bTip.X, finger1bTip.Y);
        var finger2b = new Vector2(finger2bTip.X, finger2bTip.Y);
        var finger3b = new Vector2(finger3bTip.X, finger3bTip.Y);
        var finger4b = new Vector2(finger4bTip.X, finger4bTip.Y);

        //check each fingertip's distance to the  joints
        if (Vector2.Distance(finger1, finger1b) > rockThreshold || Vector2.Distance(finger2, finger2b) > rockThreshold
        || Vector2.Distance(finger3, finger3b) > rockThreshold || Vector2.Distance(finger4, finger4b) > rockThreshold)
        {
            return false;
        }

        summonRock = true;
        return true;
    }

    private bool IsPaper(NormalizedLandmarkList landmarks)
    {
        var finger1Tip = landmarks.Landmark[8];
        var finger2Tip = landmarks.Landmark[12];
        var finger3Tip = landmarks.Landmark[16];
        var finger4Tip = landmarks.Landmark[20];
        var wristL = landmarks.Landmark[0];

        var finger1 = new Vector2(finger1Tip.X, finger1Tip.Y);
        var finger2 = new Vector2(finger2Tip.X, finger2Tip.Y);
        var finger3 = new Vector2(finger3Tip.X, finger3Tip.Y);
        var finger4 = new Vector2(finger4Tip.X, finger4Tip.Y);
        var wrist = new Vector2(wristL.X, wristL.Y);

        // Debug.Log("distances " + Vector2.Distance(finger1,wrist) + " " + Vector2.Distance(finger2,wrist) + " " 
        // + Vector2.Distance(finger3,wrist) + " " + Vector2.Distance(finger4,wrist));

        //check fingertip distance to palm
        if (Vector2.Distance(finger1, wrist) < paperThreshold || Vector2.Distance(finger2, wrist) < paperThreshold
        || Vector2.Distance(finger3, wrist) < paperThreshold || Vector2.Distance(finger4, wrist) < paperThreshold)
        {
            return false;
        }



        summonPaper = true;
        return true;
    }
}

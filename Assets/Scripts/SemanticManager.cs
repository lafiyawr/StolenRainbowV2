using Niantic.Lightship.AR.Semantics;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class SemanticManager : MonoBehaviour

{
    public ARCameraManager _cameraMan;
    public ARSemanticSegmentationManager _semanticMan;

    public TMP_Text _text;
    public RawImage _image;
    public Material _material;
    public string _preferredSegmentation = "sky";
    public GameObject rainbow;
    [SerializeField]
    private PlayableDirector _playableDirector;
    public bool spawnRainbow = false;
    public float distance;
    public bool debugSegmentation = false;

    private string _channel = "sky";

    // This is to trigger the background music fadeout
    public delegate IEnumerator SkyTapped();
    public static event SkyTapped skyTapped;


    void OnEnable()
    {
        _cameraMan.frameReceived += OnCameraFrameUpdate;
    }

    private void OnDisable()
    {
        _cameraMan.frameReceived -= OnCameraFrameUpdate;
    }


    private void OnCameraFrameUpdate(ARCameraFrameEventArgs args)
    {
        if (!_semanticMan.subsystem.running)
        {
            return;
        }

        //get the semantic texture
        Matrix4x4 mat = Matrix4x4.identity;
        var texture = _semanticMan.GetSemanticChannelTexture(_channel, out mat);


        if (texture)
        {
            //the texture needs to be aligned to the screen so get the display matrix
            //and use a shader that will rotate/scale things.
            Matrix4x4 cameraMatrix = args.displayMatrix ?? Matrix4x4.identity;
            _image.material = _material;
            _image.material.SetTexture("_SemanticTex", texture);
            _image.material.SetMatrix("_SemanticMat", mat);
        }
    }

    private float _timer = 0.0f;
    void Update()
    {


        if (!_semanticMan.subsystem.running)
        {
            return;
        }

        if (spawnRainbow)
        {









#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                var pos = Input.mousePosition;
#else
        if (Input.touches.Length > 0)
            {
            var pos = Input.touches[0].position;
#endif

                if (pos.x > 0 && pos.x < Screen.width)
                    if (pos.y > 0 && pos.y < Screen.height)
                    {
                        _timer += Time.deltaTime;
                        if (_timer > 0.05f)
                        {
                            var list = _semanticMan.GetChannelNamesAt((int)pos.x, (int)pos.y);

                            if (list.Count > 0)
                            {
                                _channel = list[0];
                              //  print(_channel);
                              //if someone touches a part of the screen that is recognized as your preferred segmentation (e.g. "sky"), start 
                              //the timeline and moves the rainbow at a certain distance away from the camera.
                                if (_channel == _preferredSegmentation)
                                {

                                    TimelineControl.StartTimeline(_playableDirector);
                                    StartCoroutine(skyTapped());

                                     var newPos = Camera.main.transform.TransformPoint(Vector3.forward * distance);
                                   // var newPos = Camera.main.transform.TransformPoint(0 ,5f, 1f * distance);
                                    var newRot = Camera.main.transform.rotation;

                                    rainbow.transform.position = newPos;
                                    rainbow.transform.rotation = newRot;

                                    rainbow.SetActive(true);
                                    //  _text.text = "rainbow appeared!";
                                    spawnRainbow = false;

                                }
                                else
                                {
                                    _text.text = _channel;
                                }


                            }
                            else
                            {
                                _text.text = "?";
                            }

                            _timer = 0.0f;
                        }
                    }
            }

            //This is just for debugging in the Editor when video playback mode is turned off and you can't trigger the sky tap. In Play Mode set the "degubSegmentation" bool to true
            // on the "SemanticManager" Gameobject after the instructions say "tap the sky" in order to trigger the animation and continue testing.

            if (debugSegmentation == true)
            {

                TimelineControl.StartTimeline(_playableDirector);

                StartCoroutine(skyTapped());
                var newPos = Camera.main.transform.TransformPoint(Vector3.forward * distance);
                var newRot = Camera.main.transform.rotation;


                rainbow.transform.position = newPos;
                rainbow.transform.rotation = newRot;

                rainbow.SetActive(true);
               // _text.text = "rainbow has appeared!";
                spawnRainbow = false;


            }



        }

    }




    public void BoolToggle()
    {
        if (spawnRainbow)
        {
            spawnRainbow = false;
        }
        else
        {
            spawnRainbow = true;
        }
    }

}

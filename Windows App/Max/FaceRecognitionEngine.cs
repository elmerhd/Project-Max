using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Face;
using Emgu.CV.Util;
using System.Drawing;
using Emgu.CV.CvEnum;
using System.IO;
using Newtonsoft.Json;

namespace Max
{
    public class FaceRecognitionEngine
    {
        private Timer Timer { get; set; }
        private MaxEngine MaxEngine { get; set; }

        private VideoCapture videoCapture;
        private CascadeClassifier haarCascade;
        private Image<Bgr, Byte> bgrFrame = null;
        private Image<Gray, Byte> detectedFace = null;
        private List<Face> faceList = new List<Face>();
        private VectorOfMat imageList = new VectorOfMat();
        private List<string> nameList = new List<string>();
        private VectorOfInt labelList = new VectorOfInt();
        private EigenFaceRecognizer recognizer;
        public string FaceName { get; set; }

        public FaceRecognitionEngine(MaxEngine maxEngine)
        {
            this.MaxEngine = maxEngine;
            Timer = new Timer();
            GetFacesList();
            videoCapture = new VideoCapture(App.GetEngine().MaxConfig.ActiveCameraIndex);
            videoCapture.SetCaptureProperty(CapProp.Fps, 30);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 450);
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 370);
            Timer.Interval = 500;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
            maxEngine.BrainEngine.Log($"Loading {nameof(FaceRecognitionEngine)}");
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ProcessFrame();
        }
        public void GetFacesList()
        {
            haarCascade = new CascadeClassifier($@"{App.GetEngine().MaxConfig.HaarCascadePath}");
            faceList.Clear();
            List<Face> faces = JsonConvert.DeserializeObject<List<Face>>(File.ReadAllText(App.GetEngine().MaxConfig.FaceListTextFile));

            foreach (Face face in faces)
            {
                face.FaceImage = new Image<Gray, byte>(App.GetEngine().MaxConfig.FacePhotosPath + "/" + face.Image + App.GetEngine().MaxConfig.ImageFileExtension);
                faceList.Add(face);
            }

            for (int i = 0; i < faceList.Count; i++) {
                imageList.Push(faceList[i].FaceImage.Mat);
                nameList.Add(faceList[i].Name);
                labelList.Push(new int[] { i });
            }
            // Train recogniser
            if (imageList.Size > 0)
            {
                recognizer = new EigenFaceRecognizer(imageList.Size);
                recognizer.Train(imageList, labelList);
            }

        }

        private void ProcessFrame()
        {
            bgrFrame = videoCapture.QueryFrame().ToImage<Bgr, Byte>();

            if (bgrFrame != null)
            {
                try
                {//for emgu cv bug
                    Image<Gray, byte> grayframe = bgrFrame.Convert<Gray, byte>();

                    Rectangle[] faces = haarCascade.DetectMultiScale(grayframe, 1.2, 10, new System.Drawing.Size(50, 50), new System.Drawing.Size(200, 200));

                    //detect face
                    //FaceName = "No face detected";
                    foreach (var face in faces)
                    {
                        bgrFrame.Draw(face, new Bgr(255, 255, 0), 2);
                        detectedFace = bgrFrame.Copy(face).Convert<Gray, byte>();
                        FaceRecognition();
                        break;
                    }
                }
                catch (Exception ex)
                {

                    //todo log
                }

            }
        }
        private void FaceRecognition()
        {
            if (imageList.Size != 0)
            {
                //Eigen Face Algorithm
                FaceRecognizer.PredictionResult result = recognizer.Predict(detectedFace.Resize(100, 100, Inter.Cubic));
                FaceName = nameList[result.Label];
                App.GetEngine().VoiceOutputEngine.Speak("Hello "+FaceName);

            }
            else
            {
                FaceName = "Please Add Face";
            }
        }
    }

    public class Face
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public Image<Gray, byte> FaceImage { get; set; }
        public string Salutation { get; set; }
    }
}

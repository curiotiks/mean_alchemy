using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.IO;
using DG.Tweening;
using UnityEngine.Events;
using TMPro; 

public enum OffsetDirection {Left, Bottom, Right, Top};
[RequireComponent(typeof(DOTween))]
public static class Utils 
{  
    public static string getDate(){
        return DateTime.Now.Month+"/"+DateTime.Now.Day+"/"+DateTime.Now.Year;
    }

    #region mac / pc open saved screenshot folder
    //https://answers.unity.com/questions/43422/how-to-implement-show-in-explorer.html
     private static void OpenInMacFileBrowser(string path)
     {
         bool openInsidesOfFolder = false;
 
         // try mac
         string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes
 
         if (Directory.Exists(macPath)) // if path requested is a folder, automatically open insides of that folder
         {
             openInsidesOfFolder = true;
         }
 
         //Debug.Log("macPath: " + macPath);
         //Debug.Log("openInsidesOfFolder: " + openInsidesOfFolder);
 
         if (!macPath.StartsWith("\""))
         {
             macPath = "\"" + macPath;
         }
         if (!macPath.EndsWith("\""))
         {
             macPath = macPath + "\"";
         }
         string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;
         //Debug.Log("arguments: " + arguments);
         try
         {
             System.Diagnostics.Process.Start("open", arguments);
         }
         catch(System.ComponentModel.Win32Exception e)
         {
             // tried to open mac finder in windows
             // just silently skip error
             // we currently have no platform define for the current OS we are in, so we resort to this
             e.HelpLink = ""; // do anything with this variable to silence warning about not using it
         }
     }
 
     private static void OpenInWinFileBrowser(string path)
     {
         bool openInsidesOfFolder = false;
 
         // try windows
         string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes
 
         if (Directory.Exists(winPath)) // if path requested is a folder, automatically open insides of that folder
         {
             openInsidesOfFolder = true;
         }
         try
         {
             System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + winPath);
         }
         catch(System.ComponentModel.Win32Exception e)
         {
             // tried to open win explorer in mac
             // just silently skip error
             // we currently have no platform define for the current OS we are in, so we resort to this
             e.HelpLink = ""; // do anything with this variable to silence warning about not using it
         }
     }
 
     public static void OpenInFileBrowser(string path)
     {
         OpenInWinFileBrowser(path);
         OpenInMacFileBrowser(path);
     }

    #endregion

    public static Color32 getDarkerColor(Color32 originColor, float ratio){
        float h;
        float s;
        float v;
        
        Color.RGBToHSV( new Color( (float)(originColor.r / 255f), (float)(originColor.g / 255f), (float)(originColor.b / 255f)), out h, out s, out v );

        // Debug.Log(h+" "+s+" "+v);
        v = v * ratio;   //darker

        Color color = Color.HSVToRGB( h, s, v);
        // Debug.Log(color);
        Color32 color32 = new Color32( (byte)(color.r * 255), (byte)(color.g *255) , (byte)(color.b *255), 255);
        // Debug.Log(color32);

        return color32;
    }

    public static int addLayerMask(int originalLayerNumber, string name){
        return originalLayerNumber |= (1 << LayerMask.NameToLayer(name));
    }

    public static Texture2D reSampleAndCrop(Texture2D source, int targetWidth, int targetHeight)
    {
        int sourceWidth = source.width;
        int sourceHeight = source.height;
        float sourceAspect = (float)sourceWidth / sourceHeight;
        float targetAspect = (float)targetWidth / targetHeight;
        int xOffset = 0;
        int yOffset = 0;
        float factor = 1;
        if (sourceAspect > targetAspect)
        { // crop width
            factor = (float)targetHeight / sourceHeight;
            xOffset = (int)((sourceWidth - sourceHeight * targetAspect) * 0.5f);
        }
        else
        { // crop height
            factor = (float)targetWidth / sourceWidth;
            yOffset = (int)((sourceHeight - sourceWidth / targetAspect) * 0.5f);
        }
        Color32[] data = source.GetPixels32();
        Color32[] data2 = new Color32[targetWidth * targetHeight];
        for (int y = 0; y < targetHeight; y++)
        {
            float yPos = y / factor + yOffset;
            int y1 = (int)yPos;
            if (y1 >= sourceHeight)
            {
                y1 = sourceHeight - 1;
                yPos = y1;
            }

            int y2 = y1 + 1;
            if (y2 >= sourceHeight)
                y2 = sourceHeight - 1;
            float fy = yPos - y1;
            y1 *= sourceWidth;
            y2 *= sourceWidth;
            for (int x = 0; x < targetWidth; x++)
            {
                float xPos = x / factor + xOffset;
                int x1 = (int)xPos;
                if (x1 >= sourceWidth)
                {
                    x1 = sourceWidth - 1;
                    xPos = x1;
                }
                int x2 = x1 + 1;
                if (x2 >= sourceWidth)
                    x2 = sourceWidth - 1;
                float fx = xPos - x1;
                var c11 = data[x1 + y1];
                var c12 = data[x1 + y2];
                var c21 = data[x2 + y1];
                var c22 = data[x2 + y2];
                float f11 = (1 - fx) * (1 - fy);
                float f12 = (1 - fx) * fy;
                float f21 = fx * (1 - fy);
                float f22 = fx * fy;
                float r = c11.r * f11 + c12.r * f12 + c21.r * f21 + c22.r * f22;
                float g = c11.g * f11 + c12.g * f12 + c21.g * f21 + c22.g * f22;
                float b = c11.b * f11 + c12.b * f12 + c21.b * f21 + c22.b * f22;
                float a = c11.a * f11 + c12.a * f12 + c21.a * f21 + c22.a * f22;
                int index = x + y * targetWidth;

                data2[index].r = (byte)r;
                data2[index].g = (byte)g;
                data2[index].b = (byte)b;
                data2[index].a = (byte)a;
            }
        }

        var tex = new Texture2D(targetWidth, targetHeight);
        tex.SetPixels32(data2);
        tex.Apply(true);
        return tex;
    }  

    public static Tweener changeTMPfontSize(TextMeshProUGUI targetTMP, float targetFontSize, float animTime = 0.5f){
        return DOTween.To( ()=> targetTMP.fontSize, x=>targetTMP.fontSize=x, targetFontSize, animTime).SetEase(Ease.OutSine);
    }

    public static Tweener changeUIposition(RectTransform targetRT, Vector2 targetPosition, float animTime = 0.5f){
        return DOTween.To( ()=> targetRT.anchoredPosition, x=>targetRT.anchoredPosition=x, targetPosition, animTime).SetEase(Ease.OutSine);
    }

    public static Tweener changeUIsize(RectTransform targetRT, Vector2 targetSize, float animTime = 0.5f){
        return DOTween.To( ()=>targetRT.sizeDelta, x=>targetRT.sizeDelta=x, targetSize, animTime).SetEase(Ease.OutSine);
    } 
    
    public static Tween changeUISizeByOffset(RectTransform targetRT, OffsetDirection offsetDirection, float value, float animTime = 0.5f){
        Tween t = null;
        if(offsetDirection == OffsetDirection.Left){
            t = DOTween.To( ()=>targetRT.offsetMin, x=>targetRT.offsetMin=x, new Vector2(value, targetRT.offsetMin.y), animTime).SetEase(Ease.OutSine);
        }else if(offsetDirection == OffsetDirection.Bottom){
            t = DOTween.To( ()=>targetRT.offsetMin, x=>targetRT.offsetMin=x, new Vector2(targetRT.offsetMin.x, value), animTime).SetEase(Ease.OutSine);
        }else if(offsetDirection == OffsetDirection.Right){
            t = DOTween.To( ()=>targetRT.offsetMax, x=>targetRT.offsetMax=x, new Vector2(value, targetRT.offsetMax.y), animTime).SetEase(Ease.OutSine);
        }else if(offsetDirection == OffsetDirection.Top){
            t = DOTween.To( ()=>targetRT.offsetMax, x=>targetRT.offsetMax=x, new Vector2(targetRT.offsetMax.x, value), animTime).SetEase(Ease.OutSine);
        }
        
        return t;
    }

    public static Tweener showTargetCanvasGroup(CanvasGroup targetUI, bool isShow, float time = 0.3f, UnityAction action = null){ 
        if(targetUI == null)
            return null;

        if(isShow)
            targetUI.gameObject.SetActive(true);    //시작에는 무조건 on

            targetUI.interactable = true;
            targetUI.blocksRaycasts = false;
            if(isShow){
                startDelayedAction(()=>{
                    targetUI.blocksRaycasts = true;
                }, time);

                return DOTween.To( ()=>targetUI.alpha, x=>targetUI.alpha=x, 1, time).SetEase(Ease.OutSine).OnComplete( ()=>{
                    if(action != null)
                        action();
                });
            }
            else{
                return DOTween.To( ()=>targetUI.alpha, x=>targetUI.alpha=x, 0, time).SetEase(Ease.OutSine).OnComplete( ()=>{
                    if(action != null)
                        action();
                });
            }
    } 

    public static T convertStringToEnum<T>(string str){
        return (T) Enum.Parse(typeof(T), str, true);
    }

    public static string convertIntToTimeString(int time){
        TimeSpan t = TimeSpan.FromSeconds(time);
        string answer = string.Format("{0:D2}:{1:D2}:{2:D2}s", 
                t.Hours, 
                t.Minutes, 
                t.Seconds);
        return answer;
    } 

    public static void startDelayedAction(UnityAction action, float waitingTime){
        int xx = 0;
        DOTween.To(()=>xx, x=>xx=x, 1, waitingTime).OnComplete( ()=>{
            if(action != null)
                action();
        });
    }

    public static Vector3 absVector3(Vector3 vector3){
        return new Vector3( Mathf.Abs(vector3.x), Mathf.Abs(vector3.y), Mathf.Abs(vector3.z));
    }

    public static void addCullingMaskToTheTargetCamera(Camera camera, string layerName) { 
        camera.cullingMask |= 1 << LayerMask.NameToLayer(layerName);
    }
     
    public static void removeCullingMaskToTheTargetCamera(Camera camera, string layerName) {
        camera.cullingMask &=  ~(1 << LayerMask.NameToLayer(layerName));
    } 

    // public static T readJsonFromFile<T>(string path, string uid)
    // { 
    //     // Debug.Log("readJsonFromFile: "+uid); 
    //     string text = null;

    //     try{
    //         text = File.ReadAllText( path + "/" + uid +".json", Encoding.UTF8 );
    //         // text = File.ReadAllText(Application.persistentDataPath+"/"+uid+".json", Encoding.UTF8);  
    //     }
    //     catch(Exception e){
    //         Debug.Log("readJsonFromFile exception\n" +e);
    //     }
    //     return JsonConvert.DeserializeObject<T>(text);
    // }    

    public static bool writeJsonFile(string path, string uid, string jsonData)
    {
        try{
            // Debug.Log(jsonData);
            File.WriteAllText(path+"/"+uid+".json", jsonData);

            // File.WriteAllText(Application.persistentDataPath+"/"+uid+".json", jsonData);
            return true;
        }catch(Exception e){
            Debug.Log(e);
            return false;            
        }
        // FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", Application.dataPath, uid), FileMode.Create);
        // byte[] data = Encoding.UTF8.GetBytes(jsonData);
        // fileStream.Write(data, 0, data.Length);
        // fileStream.Close();
    }

    public static bool writeTextFile(string name, string content){
        try{
            File.WriteAllText(Application.dataPath+"/"+name+".text", content);
            Debug.Log(name+" saved in text file");
            return true;
        }catch(Exception e){
            Debug.Log(e);
            return false;            
        }
    }

    public static string objectToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    public static T jsonToOject<T>(string jsonData)
    {
        return JsonUtility.FromJson<T>(jsonData);
    }


    public static bool CheckInternetConnection(){
        return !(Application.internetReachability == NetworkReachability.NotReachable);
    }

    public static int[] randomizeArray(int length, int startingNumber = 0){
        int[] orderArray = new int[length];      //무작위로 배열된 outsideLinesList의 index number
        for(int i=startingNumber; i<length+startingNumber; i++){
            orderArray[i] = i;
        }
        System.Random rnd=new System.Random();
        int[] randomArray = orderArray.OrderBy(x => rnd.Next()).ToArray();
        // Debug.Log(string.Join("_", randomArray.ToArray()));
        return randomArray;
    } 

    
    public static void ChangeLayers(GameObject go, string name)
    { 
        Transform[] trs = go.GetComponentsInChildren<Transform>(true);
        foreach(Transform tr in trs){
            tr.gameObject.layer = LayerMask.NameToLayer(name);
        }
    }
    
    public static void ChangeLayers(GameObject go, int layer)
    { 
        Transform[] trs = go.GetComponentsInChildren<Transform>(true);
        foreach(Transform tr in trs){
            tr.gameObject.layer = layer;
        }
    }

    public static void ChangeTags(GameObject go, string TAG){
        Transform[] trs = go.GetComponentsInChildren<Transform>(true);
        foreach(Transform tr in trs){
            tr.gameObject.tag = TAG;
        }
    } 

    public static Vector2 RotateVector2(this Vector2 v, float degrees)
    {
        return Quaternion.Euler(0, 0, degrees) * v;
    }

    public static Vector2 RotateVector3(this Vector3 v, float degrees)
    {
        return Quaternion.Euler(0, degrees, 0) * v;
    }

    public static bool checkTwoVectorsIfIdentical(Vector3 one, Vector3 two, int zeroCount = 2){
        Vector3 a = getRoundedVector3(one, zeroCount);
        Vector3 b = getRoundedVector3(two, zeroCount);

        return a == b;
    }

    public static bool checkTwoVectorListsAreSame(List<Vector3> list1, List<Vector3> list2){
        bool isSame = true;

        foreach(Vector3 vector1 in list1){
            if(!list2.Contains(vector1)){
                isSame = false;
                break;
            }
        }

        return isSame;
    }


    //0.000x라면 0으로 취급함
    public static bool isApproximatelyZero(float x){
        if(  (int)(Mathf.Abs(x) * 1000) == 0 )
            return  true;
        else
            return false;
    }

    public static Vector3 getRoundedVector3(Vector3 vector, int zeroCount){
        Vector3 vector3 = new Vector3( roundFloatNumber(vector.x, zeroCount), roundFloatNumber(vector.y, zeroCount), roundFloatNumber(vector.z, zeroCount));
        return vector3;
    }
 
 
    public static float roundFloatNumber(float x, int zeroCount){
        if(zeroCount < 1){
            Console.WriteLine("zeroCount error");
            return 0;
        }

        int num = 1;
        for(int i=0; i<zeroCount; i++){
            num = num * 10;
        }

        //0.01234
        // int n = (int)(x * num);
        // float f = (float)n / num;

        

        //Debug.Log("origin: "+x+" num: "+num +" n:"+n+" f:"+f); 

        return Mathf.Round(x*num)/num;
    }

    public static Rect getUV_RectForWebcamTexture(){
        Rect rect = new Rect(1, 1, 0, 0);
        #if UNITY_STANDALONE || UNITY_EDITOR
            rect = new Rect(0, 1, 1, -1);
        #elif UNITY_ANDROID 
            rect = new Rect(1, 1, -1, -1);
        #elif UNITY_IOS
            rect = new Rect(1, 0, -1, 1);
        #endif

        return rect;
    }

    public class Combination
    {
        private long n = 0;
        private long k = 0;
        private long[] data = null;

        public Combination(long n, long k)
        {
            if (n < 0 || k < 0)
            {
                throw new Exception("Negative parameter in constructor");
            }

            this.n = n;
            this.k = k;
            this.data = new long[k];

            for (long i = 0; i < k; ++i)
            {
                this.data[i] = i;
            }
        }

        public Combination Successor()
        {
            if (this.data.Length == 0 ||
                this.data[0] == this.n - this.k)
            {
                return null;
            }

            Combination answer = new Combination(this.n, this.k);

            long i;
            for (i = 0; i < this.k; ++i)
            {
                answer.data[i] = this.data[i];
            }

            for (i = this.k - 1; i > 0 && answer.data[i] == this.n - this.k + i; --i) ;

            ++answer.data[i];

            for (long j = i; j < this.k - 1; ++j)
            {
                answer.data[j + 1] = answer.data[j] + 1;
            }

            return answer;
        }

        public string[] ApplyTo(string[] strarr)
        {
            if (strarr.Length != this.n)
            {
                throw new Exception("Bad array size");
            }

            string[] result = new string[this.k];

            for (long i = 0; i < result.Length; ++i)
            {
                result[i] = strarr[this.data[i]];
            }

            return result;
        }

        public static long Choose(long n, long k)
        {
            if (n < 0 || k < 0)
            {
                throw new Exception("Invalid negative parameter in Choose()");
            }

            if (n < k)
            {
                return 0;
            }

            if (n == k)
            {
                return 1;
            }

            long delta, iMax;

            if (k < n - k)
            {
                delta = n - k;
                iMax = k;
            }
            else
            {
                delta = k;
                iMax = n - k;
            }

            long answer = delta + 1;

            for (long i = 2; i <= iMax; ++i)
            {
                checked { answer = (answer * (delta + i)) / i; }
            }

            return answer;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (long i = 0; i < this.k; ++i)
            {
                sb.AppendFormat("{0} ", this.data[i]);
            }

            sb.Remove( sb.Length-1, 1);
            return sb.ToString();
        }
    }

    public class Permutation
    {
        private int[] data = null;
        private int order = 0;

        public Permutation(int n)
        {
            this.data = new int[n];
            for (int i = 0; i < n; ++i)
            {
                this.data[i] = i;
            }

            this.order = n;
        }

        public Permutation Successor()
        {
            Permutation result = new Permutation(this.order);

            int left, right;
            for (int k = 0; k < result.order; ++k)  // Step #0 - copy current data into result
            {
                result.data[k] = this.data[k];
            }

            left = result.order - 2;  // Step #1 - Find left value 
            while ((result.data[left] > result.data[left + 1]) && (left >= 1))
            {
                --left;
            }

            if ((left == 0) && (this.data[left] > this.data[left + 1]))
            {
                return null;
            }

            right = result.order - 1;  // Step #2 - find right; first value > left
            while (result.data[left] > result.data[right])
            {
                --right;
            }

            int temp = result.data[left];  // Step #3 - swap [left] and [right]
            result.data[left] = result.data[right];
            result.data[right] = temp;


            int i = left + 1;              // Step #4 - order the tail
            int j = result.order - 1;

            while (i < j)
            {
                temp = result.data[i];
                result.data[i++] = result.data[j];
                result.data[j--] = temp;
            }

            return result;
        }

        internal static long Choose(int length)
        {
            long answer = 1;

            for (int i = 1; i <= length; i ++)
            {
                checked { answer = answer * i; }
            }

            return answer;
        }

        public string[] ApplyTo(string[] arr)
        {
            if (arr.Length != this.order)
            {
                return null;
            }

            string[] result = new string[arr.Length];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = arr[this.data[i]];
            }

            return result;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < this.order; ++i)
            {
                sb.Append(this.data[i].ToString() + " ");
            } 
            
            sb.Remove( sb.Length-1, 1);
            return sb.ToString();
        }

    }
    
} 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using Text = UnityEngine.UI.Text;
using Debug = UnityEngine.Debug;
using System.Net.Sockets;

namespace PMT
{
    public static class Utils
    {
        #region WEB, REQUEST, INTERNET, CONNECTION
        public static bool CheckRequestError(UnityWebRequest request) =>
            request.result.Equals(UnityWebRequest.Result.ConnectionError) || request.result.Equals(UnityWebRequest.Result.DataProcessingError)
            || request.result.Equals(UnityWebRequest.Result.ProtocolError);
        public static void CalcISpeed(string link = "https://www.google.com/")
        {
            Stopwatch watch = new Stopwatch(); //using system.diagnostics
            watch.Start();
            WebClient web = new WebClient();
            //https://www.berlin.de/en/
            //https://www.tps.uz/
            //https://www.google.com/
            //http://file.vronica.uz:8081/files/Projects/USAID/Short%20videos/end.mp3
            byte[] bytes = web.DownloadData(link);
            watch.Stop();
            double sec = watch.Elapsed.TotalSeconds;
            double speed = (8 * bytes.Count() / 1024) / sec;
            UnityEngine.Debug.Log(speed + " Kb / S");
        }
        public static IEnumerator UploadToServer(string serverPath, byte[] data)
        {
            WWWForm form = new WWWForm();

            form.AddBinaryData("myimage", data, "imageName.png", "image/png");

            UnityWebRequest www = UnityWebRequest.Post(serverPath, form);

            yield return www.SendWebRequest();

            if (CheckRequestError(www))
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                UnityEngine.Debug.Log("Form upload complete! " + www.downloadHandler.text);

            }
        }

        public class WebFileWorks
        {
            string key, fileSavePath;
            public WebFileWorks(string key)
            {
                this.key = key;
                if (!Exists())
                {
                    PlayerPrefs.SetString(key, "");
                }
            }
            public void WriteAllText(string data)
            {
                PlayerPrefs.SetString(key, data);
                PlayerPrefs.Save();
            }
            public string ReadAllText()
            {
                return PlayerPrefs.GetString(key);
            }
            public bool Exists()
            {
                return PlayerPrefs.HasKey(key);
            }
        }

        public static IEnumerator Write()
        {
            yield return null;
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {

                    return ip.ToString();
                }
            }
            throw new System.Exception("No network adapters with an IPv4 address in the system!");
        }

        #endregion
        #region UI
        public static bool IsPointerOverUI()
        {
            //(x,y) Screen touch or mouse down positions
            PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }
        public static bool IsPointerOverUI(float x, float y)
        {
            //(x,y) Screen touch or mouse down positions
            PointerEventData eventData = new PointerEventData(EventSystem.current) { position = new Vector2(x, y) };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }
        public static bool IsPointerOverUI(Vector2 touchPos)
        {
            //(x,y) Screen touch or mouse down positions
            PointerEventData eventData = new PointerEventData(EventSystem.current) { position = touchPos };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }
        /// <summary>
        /// Copy paste height width from Text prefered size
        /// </summary>
        /// <param name="rt"></param>
        public static void CopyRectSize(this RectTransform rt, RectTransform target)
        {
            try
            {
                //rt.sizeDelta = GetRectSize(target);
                Canvas.ForceUpdateCanvases();
                rt.sizeDelta = target.sizeDelta;
            }
            catch
            {
                Debug.LogError(rt.name + " : rect transform CopyChildTextSize error");
            }
        }
        public static Vector2 GetRectSize(RectTransform rt)
        {
            try
            {
                Text childText = rt.GetComponentInChildren<Text>();
                float GetTextHeight(Text text) => text.cachedTextGenerator.lines.Count * (text.fontSize + text.lineSpacing * 2);
                return new Vector2(childText.preferredWidth, GetTextHeight(childText));
            }
            catch
            {
                Debug.LogError(rt.name + " : rect transform CopyChildTextSize error");
                return Vector2.zero;
            }
        }
        public static void CopyMaxContentSizeOfChild(this RectTransform rt)
        {
            try
            {
                rt.sizeDelta = GetMaxContentSizeOfChild(rt);
            }
            catch
            {
                Debug.LogError(rt.name + " : rect transform CopyChildTextSize error");
            }
        }
        public static Vector2 GetMaxContentSizeOfChild(RectTransform rt)
        {
            Vector2 maxSize = Vector2.zero;
            try
            {
                foreach (RectTransform crt in rt)
                {
                    Canvas.ForceUpdateCanvases();

                    Vector2 childSize = crt.sizeDelta;
                    if (maxSize.magnitude < childSize.magnitude)
                        maxSize = childSize;
                }
            }
            catch
            {
                Debug.LogError(rt.name + " : rect transform CopyChildTextSize error");
                return Vector2.zero;
            }
            return maxSize;
        }
        #endregion
        //ru uz ru-uz
        #region FileWorks
        public static void DeleteAllFilesInDirectory(string dirPath, string ext)
        {
            DirectoryInfo di = new DirectoryInfo(dirPath);
            string searchPattern = "*." + ext;
            FileInfo[] finfo = di.GetFiles(searchPattern);
            foreach (FileInfo file in finfo)
            {
                File.Delete(file.FullName);
            }
        }
        #endregion

        #region Raycast
        public static Transform HitRaycast_GetHitTransform(Vector3 origin, Vector3 direction)
        {
            Transform tr = null;
            Ray ray = new Ray(origin, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
                tr = hit.transform;
            return tr;
        }
        public static Transform CameraRaycast_GetHitTransform()
        {
            Transform tr = null;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
                tr = hit.transform;
            return tr;
        }
        public static Transform CameraRaycast_GetHitTransform(LayerMask layer)
        {
            Transform tr = null;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100, layer))
                tr = hit.transform;
            return tr;
        }
        #endregion
        #region Helper functions
        public static async void AsyncInvoke(float delay, Action toDo)
        {
            while (delay > 0)
            {
                delay -= Time.deltaTime;
                await Task.Yield();
            }
            toDo?.Invoke();
        }
        public static IEnumerator Invoke(float time, Action toDo)
        {
            yield return new WaitForSeconds(time);
            toDo?.Invoke();
        }
        public static IEnumerator InvokeUnscaled(float time, Action toDo)
        {
            yield return new WaitForSecondsRealtime(time);
            toDo?.Invoke();
        }
        public static void InvokeMono(MonoBehaviour mono, float time, Action toDo)
        {
            mono.StartCoroutine(Invoke(time, toDo));
        }
        public static void InvokeAfter(MonoBehaviour context, Action action, float time, bool unscaledTime = false)
        {
            context.StartCoroutine(_invokeR(action, time, unscaledTime));
        }
        static IEnumerator _invokeR(Action action, float time, bool unscaledTime)
        {
            if (unscaledTime)
                yield return new WaitForSecondsRealtime(time);
            else
                yield return new WaitForSeconds(time);
            action();
        }
        #endregion
        #region Tween
        public static void FTween(MonoBehaviour mb, Action<float> action, float time)
        {
            mb.StartCoroutine(FTweenValue(action, time));
            static IEnumerator FTweenValue(Action<float> action, float time)
            {
                if (time <= 0)
                {
                    yield break;
                }
                float elapsed = 0;
                while (elapsed < time)
                {
                    elapsed = Mathf.Clamp(elapsed + Time.fixedDeltaTime, 0, time);
                    action(Mathf.Pow(elapsed / time, 1f / 4f));
                    yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
                }
            }
        }
        public static void LazySetParent(this Transform tr, Transform parent, float time)
        {
            tr.SetParent(parent);
            Vector3 startPos = tr.position;
            GameObject tempParent = new GameObject();
            tempParent.transform.position = tr.position;
            tempParent.transform.rotation = tr.rotation;
            void ToDo(float t)
            {
                tempParent.transform.position = Vector3.Lerp(startPos, parent.position, t);
                if (t == 1)
                {
                    foreach (Transform c in tempParent.transform)
                    {
                        c.SetParent(tr);
                    }
                    GameObject.Destroy(tempParent);
                }
            }

            foreach (Transform c in tr)
            {
                c.SetParent(tempParent.transform);
            }
            FTween(tr.GetComponent<MonoBehaviour>(), ToDo, time);
        }
        public static void LazySetParent(this RectTransform rt, Transform parent, float time)
        {
            Vector3 startPos = rt.position;
            RectTransform tempParent = GameObject.Instantiate(rt);
            tempParent.gameObject.DestroyAllChilds();


            rt.SetParent(parent);
            tempParent.position = startPos;
            void ToDo(float t)
            {
                tempParent.transform.position = Vector3.Lerp(startPos, rt.position, t);
                if (t == 1)
                {
                    tempParent.SetParentChilds(rt);

                    GameObject.Destroy(tempParent.gameObject);
                }
            }

            rt.SetParentChilds(tempParent);
            tempParent.SetParent(rt);
            tempParent.position = startPos;
            tempParent.sizeDelta = rt.sizeDelta;
            tempParent.transform.rotation = rt.rotation;

            FTween(rt.GetComponent<MonoBehaviour>(), ToDo, time);
        }
        #endregion

        #region List
        public static List<T> Shuffle<T>(List<T> list)
        {
            List<T> last = new List<T>();
            foreach (var item in list)
            {
                last.Add(item);
            }
            List<T> product = new List<T>();
            for (int i = 0; i < list.Count; i++)
            {
                T temp = last[Random.Range(0, last.Count)];
                product.Add(temp);
                last.Remove(temp);
            }
            return product;
        }
        public static bool IsRange<T>(this T[] arr, int i)
        {
            return i >= 0 && i < arr.Length;
        }
        public static bool IsRange<T>(this List<T> arr, int i)
        {
            return i >= 0 && i < arr.Count;
        }
        public static bool IsRange<T>(this LinkedList<T> arr, int i)
        {
            return i >= 0 && i < arr.Count;
        }

        public static T GetRandom<T>(this T[] arr)
        {
            return arr[Random.Range(0, arr.Length)];
        }
        public static T GetRandom<T>(this List<T> arr)
        {
            return arr[Random.Range(0, arr.Count)];
        }
        public static void CopyTo<T>(List<T> from, List<T> to) where T : struct
        {
            if(to == null)
                to = new List<T>();
            for (int i = 0; i < from.Count; i++)
            {
                if (to.Count <= i)
                    to.Add(from[i]);
                else
                    to[i] = from[i];
                
            }
        }

        #endregion
        #region GameObject
        public static void DestroyAllChilds(this GameObject go)
        {
            List<GameObject> childObjs = new List<GameObject>();
            foreach (Transform item in go.transform)
            {
                childObjs.Add(item.gameObject);
            }
            foreach (var item in childObjs)
            {
                GameObject.Destroy(item);
            }
        }
        #endregion
        #region Transform
        public static void SetVectorX(this Transform tform, float to)
        {
            Vector3 v3 = tform.position;
            tform.position = new Vector3(to, v3.y, v3.z);
        }
        public static void SetVectorY(this Transform tform, float to)
        {
            Vector3 v3 = tform.position;
            tform.position = new Vector3(v3.x, to, v3.z);
        }
        public static void SetVectorZ(this Transform tform, float to)
        {
            Vector3 v3 = tform.position;
            tform.position = new Vector3(v3.x, v3.y, to);
        }
        public static void Abs(this Transform tform)
        {
            tform.transform.position = new Vector3(Mathf.Abs(tform.transform.position.x), Mathf.Abs(tform.transform.position.y), Mathf.Abs(tform.transform.position.z));
        }
        public static void SetParentChilds(this Transform tr, Transform parent)
        {
            List<Transform> childs = new List<Transform>();
            foreach (Transform item in tr)
            {
                childs.Add(item);
            }
            foreach (Transform item in childs)
            {
                item.SetParent(parent);
            }
        }
        #endregion
        #region Vector
        public static void AddVector(this ref Vector3 v3, Vector3 add)
        {
            v3 += add;
        }
        public static void SetVector(this ref Vector3 v3, float x = float.NaN, float y = float.NaN, float z = float.NaN)
        {
            v3.x = x.Equals(float.NaN) ? v3.x : x;
            v3.y = y.Equals(float.NaN) ? v3.y : y;
            v3.z = z.Equals(float.NaN) ? v3.z : z;
        }
        public static Vector3 ChangeAndGetVector(this Vector3 v3, float x = float.NaN, float y = float.NaN, float z = float.NaN)
        {
            v3.x = x.Equals(float.NaN) ? v3.x : x;
            v3.y = y.Equals(float.NaN) ? v3.y : y;
            v3.z = z.Equals(float.NaN) ? v3.z : z;
            return v3;
        }
        public static Vector3 Abs(this Vector3 v3)
        {
            return new Vector3(Mathf.Abs(v3.x), Mathf.Abs(v3.y), Mathf.Abs(v3.z));
        }
        public static Vector3 Divide(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.x / b.x,
                a.y / b.y,
                a.z / b.z
                );
        }
        public static Vector3 V3Lerp(Vector3 v0, Vector3 v1, Vector3 t)
        {
            return new Vector3(
                Mathf.Lerp(v0.x, v1.x, t.x),
                Mathf.Lerp(v0.y, v1.y, t.y),
                Mathf.Lerp(v0.z, v1.z, t.z)
            );
        }
        #endregion
        #region Float
        public static float abs(this float value) => Mathf.Abs(value);
        #endregion
        #region string
        public static bool CheckForFIO(string name)
        {
            string lowerName = name.ToLower();
            const int wordCount = 2;
            const string ww = "qwertyuiopasdfghjkl'zxcvbnm,.éöóêåíãøùçõúôûâàïðîëäæýÿ÷ñìèòüáþ.¸- ???¢";
            bool wordCountCheck = lowerName.Count(str => str == ' ') >= wordCount;
            bool wwCheck = true;
            foreach (var item in lowerName)
            {
                if (ww.IndexOf(item) < 0)
                {
                    wwCheck = false;
                    break;
                }
            }
            return wordCountCheck && wwCheck;
        }
        #endregion

        #region Math
        
        #endregion
    }
}
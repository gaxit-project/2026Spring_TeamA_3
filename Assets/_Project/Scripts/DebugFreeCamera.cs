#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadLab
{
    /// <summary>
    /// エディタ専用フリーカメラ（幽体離脱）
    /// [1]キーでオン/オフ切り替え、UI全非表示、WASD/EQ/Shiftでカメラ移動
    /// </summary>
    public class DebugFreeCamera : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 10.0f;
        [SerializeField] private float fastMultiplier = 2.5f;
        [SerializeField] private float lookSpeed = 2.0f;

        private static DebugFreeCamera instance;
        private Camera freeCamera;
        private Camera originalCamera;
        private Canvas[] cachedCanvases;
        private bool isFreeCameraActive = false;
        private float yaw;
        private float pitch;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInit()
        {
            if (instance == null)
            {
                var go = new GameObject("[DebugFreeCamera]");
                instance = go.AddComponent<DebugFreeCamera>();
                DontDestroyOnLoad(go);
            }
        }

        private void Update()
        {
            // [1] キーでトグル切り替え
            if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                ToggleFreeCamera();
            }

            if (isFreeCameraActive)
            {
                UpdateCameraMovement();
            }
        }

        private void ToggleFreeCamera()
        {
            isFreeCameraActive = !isFreeCameraActive;

            if (isFreeCameraActive)
            {
                EnterFreeCamera();
            }
            else
            {
                ExitFreeCamera();
            }
        }

        private void EnterFreeCamera()
        {
            originalCamera = Camera.main;
            if (originalCamera == null)
            {
                originalCamera = FindFirstObjectByType<Camera>();
            }

            if (freeCamera == null)
            {
                GameObject camObj = new GameObject("FreeCameraInstance");
                freeCamera = camObj.AddComponent<Camera>();
            }

            if (originalCamera != null)
            {
                freeCamera.CopyFrom(originalCamera);
                freeCamera.transform.position = originalCamera.transform.position;
                freeCamera.transform.rotation = originalCamera.transform.rotation;
                originalCamera.enabled = false;
            }

            freeCamera.gameObject.SetActive(true);
            yaw = freeCamera.transform.eulerAngles.y;
            pitch = freeCamera.transform.eulerAngles.x;

            // 全Canvas(UI)を非表示化
            cachedCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in cachedCanvases)
            {
                if (canvas != null) canvas.enabled = false;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void ExitFreeCamera()
        {
            if (originalCamera != null)
            {
                originalCamera.enabled = true;
            }

            if (freeCamera != null)
            {
                Destroy(freeCamera.gameObject);
                freeCamera = null;
            }

            // UIを復元
            if (cachedCanvases != null)
            {
                foreach (var canvas in cachedCanvases)
                {
                    if (canvas != null) canvas.enabled = true;
                }
                cachedCanvases = null;
            }
        }

        private void UpdateCameraMovement()
        {
            if (freeCamera == null) return;

            // マウス回転
            if (Mouse.current != null)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                yaw += mouseDelta.x * lookSpeed * 0.1f;
                pitch -= mouseDelta.y * lookSpeed * 0.1f;
                pitch = Mathf.Clamp(pitch, -89f, 89f);
                freeCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }

            // 移動
            if (Keyboard.current != null)
            {
                Vector3 moveDir = Vector3.zero;
                if (Keyboard.current.wKey.isPressed) moveDir += freeCamera.transform.forward;
                if (Keyboard.current.sKey.isPressed) moveDir -= freeCamera.transform.forward;
                if (Keyboard.current.dKey.isPressed) moveDir += freeCamera.transform.right;
                if (Keyboard.current.aKey.isPressed) moveDir -= freeCamera.transform.right;
                if (Keyboard.current.eKey.isPressed || Keyboard.current.spaceKey.isPressed) moveDir += Vector3.up;
                if (Keyboard.current.qKey.isPressed || Keyboard.current.ctrlKey.isPressed) moveDir -= Vector3.up;

                float speed = moveSpeed;
                if (Keyboard.current.shiftKey.isPressed)
                {
                    speed *= fastMultiplier;
                }

                freeCamera.transform.position += moveDir.normalized * speed * Time.unscaledDeltaTime;
            }
        }

        private void OnDestroy()
        {
            if (isFreeCameraActive)
            {
                ExitFreeCamera();
            }
        }
    }
}
#endif

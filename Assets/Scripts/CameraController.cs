using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CameraType
{
    MainCamera = 0,
    SubCamera = 1
}

public enum CameraPosType
{
    ThirdPerson = 0,
    Orthographic = 1
}

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera, _subCamera;

    // 位置
    private Vector3[] _cameraPositions = new Vector3[System.Enum.GetValues(typeof(CameraPosType)).Length];

    // 角度
    private Quaternion[] _cameraRotations = new Quaternion[System.Enum.GetValues(typeof(CameraPosType)).Length];

    private Coroutine _changeCameraPos;

    private float _transitionDuration = 0.5f;
    [SerializeField] private float _mouseSensitivity;// マウス感度

    private float _minPitch = -40.0f;// 下を向ける限界角度
    private float _maxPitch = 40.0f; // 上を向ける限界角度
    private float _rotationX = 0.0f; // 現在の上下の回転角を記録する変数

    private float _minYaw = -45.0f; // 左を向ける限界角度（正面が0度）
    private float _maxYaw = 45.0f;  // 右を向ける限界角度（正面が0度）
    private float _rotationY = 0.0f;// 現在の左右の回転角を記録する変数

    private void Awake()
    {
        // 位置
        _cameraPositions[(int)CameraPosType.ThirdPerson] = new Vector3(0f, 2.17f, -1.75f);// 三人称
        _cameraPositions[(int)CameraPosType.Orthographic] = new Vector3(0f, 2f, 0.4f);   // 平行投影

        // 角度
        _cameraRotations[(int)CameraPosType.ThirdPerson] = Quaternion.Euler(28.954f, 0f, 0f);// 三人称
        _cameraRotations[(int)CameraPosType.Orthographic] = Quaternion.Euler(90f, 0f, 0f);   // 平行投影（scale:1.5）
    }

    //=================================== カメラの変更処理 =====================================
    /// <summary>
    /// メインカメラに変更（一人称視点）
    /// </summary>
    public void ChangeToMainCamera() => ChangeCamera(CameraType.MainCamera);

    /// <summary>
    /// サブカメラに変更（三人称視点）
    /// </summary>
    public void ChangeToSubCamera() => ChangeCamera(CameraType.SubCamera);

    // カメラ変更処理
    private void ChangeCamera(CameraType type)
    {
        switch (type)
        {
            case CameraType.MainCamera:
                Debug.Log("メインカメラに変更：通常状態");

                _mainCamera.depth = 1;
                _subCamera.depth = 0;

                // UI制御はGameControllerで行う

                break;

            case CameraType.SubCamera:
                Debug.Log("サブカメラに変更：銃撃状態");

                _mainCamera.depth = 0;
                _subCamera.depth = 1;

                // UI制御はGameControllerで行う

                break;

            default:
                Debug.Log("太陽するカメラが存在しません。");
                break;
        }
    }

    //=================================== カメラの視点変換処理 =====================================
    /// <summary>
    /// メインカメラを動かす（三人称視点）
    /// </summary>
    public void ChangeToThirdPerson() => TriggerCameraChange(CameraPosType.ThirdPerson);

    /// <summary>
    /// メインカメラを動かす（平行投影モード）
    /// </summary>
    public void ChangeToOrthographic() => TriggerCameraChange(CameraPosType.Orthographic);

    // 視点変更処理を実行
    private void TriggerCameraChange(CameraPosType type)
    {
        if (_changeCameraPos != null) StopCoroutine(_changeCameraPos);
        _changeCameraPos = StartCoroutine(ChangeCameraPos(type));
    }

    // 視点変更処理
    private IEnumerator ChangeCameraPos(CameraPosType cameraPosType)
    {
        int index = (int)cameraPosType;

        // スタート時の位置・角度
        Vector3 startPos = _mainCamera.transform.position;
        Quaternion startRot = _mainCamera.transform.rotation;

        // ゴール（目標）の位置・角度
        Vector3 targetPos = _cameraPositions[index];
        Quaternion targetRot = _cameraRotations[index];

        float elapsedTime = 0f;

        // 指定した時間（_transitionDuration）をかけて、なめらかに変化させるループ処理
        while (elapsedTime < _transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _transitionDuration;

            // 位置と角度を線形補間（Lerp）
            _mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            _mainCamera.transform.rotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        // 最後に完全に目標値に一致させる（ズレ防止）
        _mainCamera.transform.position = targetPos;
        _mainCamera.transform.rotation = targetRot;

        // コルーチン終了時に初期化
        _changeCameraPos = null;
    }

    //=================================== マウス制御 =====================================
    /// <summary>
    /// マウス操作でカメラを動かす（銃撃モード時に処理）
    /// </summary>
    public void MoveCamera()
    {
        if (Mouse.current == null) return;

        // MoveCamera関数の中
        float mouseX = Mouse.current.delta.x.ReadValue() * _mouseSensitivity * 0.1f;
        float mouseY = Mouse.current.delta.y.ReadValue() * _mouseSensitivity * 0.1f;

        // 左右の回転：入力値を蓄積し、Clampで限界角度の範囲内に制限する
        _rotationY += mouseX; // マウスを右に動かす(プラス)と、カメラは右を向く(プラス回転)ため加算
        _rotationY = Mathf.Clamp(_rotationY, _minYaw, _maxYaw);

        // 上下の回転：入力値を蓄積し、Clampで限界角度の範囲内に制限する
        _rotationX -= mouseY; // マウスを上に動かす(プラス)と、カメラは上を向く(マイナス回転)ため減算
        _rotationX = Mathf.Clamp(_rotationX, _minPitch, _maxPitch);

        _subCamera.transform.eulerAngles = new Vector3(_rotationX, _rotationY, 0.0f);
    }
}

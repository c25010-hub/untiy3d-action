using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CameraPosType
{
    _firstPerson = 0,
    _thirdPerson = 1,
    _orthographic = 2
}

public class CameraController : MonoBehaviour
{
    // 移動
    private Vector3[] _cameraPositions = new Vector3[3]
    {
        new Vector3(0f, 0f, 0f),// 一人称
        new Vector3(0f, 0f, 0f),// 三人称
        new Vector3(0f, 0f, 0f) // 平行投影
    };

    // 回転
    private Quaternion[] _cameraRotations = new Quaternion[3]
    {
        Quaternion.Euler(0f, 0f, 0f),// 一人称
        Quaternion.Euler(0f, 0f, 0f),// 三人称
        Quaternion.Euler(0f, 0f, 0f) // 平行投影
    };

    private Coroutine _changeCameraPos;

    private float _transitionDuration = 0.5f;
    [SerializeField] private float _mouseSensitivity;// マウス感度

    private float _minPitch = -40.0f;// 下を向ける限界角度
    private float _maxPitch = 40.0f; // 上を向ける限界角度
    private float _rotationX = 0.0f; // 現在の上下の回転角を記録する変数

    private float _minYaw = -45.0f; // 左を向ける限界角度（正面が0度）
    private float _maxYaw = 45.0f;  // 右を向ける限界角度（正面が0度）
    private float _rotationY = 0.0f;// 現在の左右の回転角を記録する変数

    private bool _readyToFire = false;
    public bool ReadyToFire => _readyToFire;

    //=================================== カメラの視点変換処理 =====================================
    /// <summary>
    /// 一人称視点（こいつの場合だけ UI も同時に動かす）
    /// </summary>
    public void ChangeToFirstPerson() => TriggerCameraChange(CameraPosType._firstPerson);

    /// <summary>
    /// 三人称視点
    /// </summary>
    public void ChangeToThirdPerson() => TriggerCameraChange(CameraPosType._thirdPerson);

    /// <summary>
    /// 平行投影（上から見下ろす）
    /// </summary>
    public void ChangeToOrthographic() => TriggerCameraChange(CameraPosType._orthographic);

    // 視点変更処理を実行
    private void TriggerCameraChange(CameraPosType type)
    {
        if (_changeCameraPos != null) StopCoroutine(_changeCameraPos);
        _changeCameraPos = StartCoroutine(ChangeCameraPos(type));

        _readyToFire = false;
    }

    // 視点変更処理
    private IEnumerator ChangeCameraPos(CameraPosType cameraPosType)
    {
        int index = (int)cameraPosType;

        // スタート時の位置・角度
        Vector3 startPos = this.gameObject.transform.localPosition;
        Quaternion startRot = this.gameObject.transform.localRotation;

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
            this.gameObject.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            this.gameObject.transform.localRotation = Quaternion.Lerp(startRot, targetRot, t);

            // 一人称視点の場合、UIも同時に開く
            if (cameraPosType == CameraPosType._firstPerson)
            {
                // 射撃準備完了！
                _readyToFire = true;
            }
            else
            {

            }

            yield return null;
        }

        // 最後に完全に目標値に一致させる（ズレ防止）
        this.gameObject.transform.localPosition = targetPos;
        this.gameObject.transform.localRotation = targetRot;

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

        transform.localEulerAngles = new Vector3(_rotationX, _rotationY, 0.0f);
    }

    private void Update()
    {
        MoveCamera();
    }
}

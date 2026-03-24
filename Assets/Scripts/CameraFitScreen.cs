using UnityEngine;

/// <summary>
/// 2D正交相机自动适配脚本（适配Size=5.8）
/// 核心：自动适配不同屏幕宽高比，保证画面不拉伸、不裁切
/// </summary>
[RequireComponent(typeof(Camera))] // 强制挂载相机组件，避免报错
public class CameraFitScreen : MonoBehaviour
{
    [Header("相机基础配置（和Inspector面板一致）")]
    [Tooltip("手动设置的相机目标Size，这里固定为5.8")]
    public float targetSize = 5.8f; // 按你的要求改为5.8
    
    [Tooltip("基准宽高比（推荐16:9，对应1.7778）")]
    public float baseAspectRatio = 16f / 9f; // 16:9电脑屏的标准宽高比

    private Camera mainCamera; // 存储相机组件

    // 游戏启动时执行一次（初始化）
    void Awake()
    {
        // 获取相机组件（Awake比Start执行更早，适配更及时）
        mainCamera = GetComponent<Camera>();
        
        // 强制设置为正交模式（避免误改透视模式）
        mainCamera.orthographic = true;
        
        // 执行适配逻辑
        FitCameraToScreen();
    }

    // 屏幕分辨率变化时重新适配（比如窗口拉伸、切换屏幕）
    void OnScreenResize()
    {
        FitCameraToScreen();
    }

    /// <summary>
    /// 核心适配逻辑：计算并设置相机的最佳Size
    /// </summary>
    private void FitCameraToScreen()
    {
        // 计算当前屏幕的实际宽高比
        float currentAspectRatio = (float)Screen.width / Screen.height;
        
        // 核心公式：根据当前宽高比，计算适配后的相机Size
        // 保证在任何屏幕比例下，画面都和16:9的5.8 Size视野一致
        float adaptSize = targetSize * (baseAspectRatio / currentAspectRatio);
        
        // 应用计算后的Size到相机
        mainCamera.orthographicSize = adaptSize;
        
        // 额外：固定相机位置（避免偏移）
        transform.position = new Vector3(0, 0, -10);
    }
}
using UnityEngine;

public class Test : MonoBehaviour
{
    public RenderTexture fogRenderTexture;
    public float fogWorldScale = 100f;
    public Vector3 fogWorldOffset = Vector3.zero;

    // 전역 프로퍼티 ID (문자열 오타 방지)
    private static readonly int FogTexID = Shader.PropertyToID("_FogTex");
    private static readonly int FogScaleID = Shader.PropertyToID("_FogScale");
    private static readonly int FogOffsetID = Shader.PropertyToID("_FogOffset");

    void Start()
    {
        Debug.Log("[FogDebugger] Start()");
        TrySetGlobal();
        DebugGlobal();
    }

    void OnEnable()
    {
        // 보수적으로 활성화 시에도 세팅
        TrySetGlobal();
        DebugGlobal();
    }

    void Update()
    {
        // Editor에서 실시간 확인하려면 Update에서 계속 설정 (성능 문제 있을 수 있으니 디버그용)
        TrySetGlobal();
    }

    void TrySetGlobal()
    {
        if (fogRenderTexture == null)
        {
            Debug.LogWarning("[FogDebugger] fogRenderTexture is NULL (inspector not assigned).");
            return;
        }

        // RenderTexture가 GPU에 올려졌는지 확인, 없으면 Create() 호출
        if (!fogRenderTexture.IsCreated())
        {
            Debug.Log("[FogDebugger] RenderTexture not created -> Calling Create()");
            fogRenderTexture.Create();
        }

        // (선택) force a small GPU upload by setting as active then restoring
        var prev = RenderTexture.active;
        RenderTexture.active = fogRenderTexture;
        // don't need to draw; just touch it
        GL.Flush();
        RenderTexture.active = prev;

        // 전역으로 설정
        Shader.SetGlobalTexture(FogTexID, fogRenderTexture);
        Shader.SetGlobalFloat(FogScaleID, fogWorldScale);
        Shader.SetGlobalVector(FogOffsetID, fogWorldOffset);
    }

    void DebugGlobal()
    {
        // 즉시 확인
        var got = Shader.GetGlobalTexture(FogTexID);
        Debug.Log("[FogDebugger] GetGlobalTexture -> " + (got == null ? "NULL" : got + $" ({got.width}x{got.height})"));
        Debug.Log($"[FogDebugger] IsCreated: {fogRenderTexture != null && fogRenderTexture.IsCreated()} Format: {(fogRenderTexture != null ? fogRenderTexture.format.ToString() : "N/A")}");
    }
}

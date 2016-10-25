using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshFilter))]
[CanEditMultipleObjects]
public class MeshFilterPreview : Editor
{
    private PreviewRenderUtility _previewRenderUtility;
    private MeshFilter _targetMeshFilter;
    private MeshRenderer _targetMeshRenderer;

    private Vector2 _drag;

    private void ValidateData()
    {
        if (_previewRenderUtility == null)
        {
            _previewRenderUtility = new PreviewRenderUtility();

            _previewRenderUtility.m_Camera.transform.position = new Vector3(0, 0, -6);
            _previewRenderUtility.m_Camera.transform.rotation = Quaternion.identity;
        }

        _targetMeshFilter = target as MeshFilter;
        _targetMeshRenderer = _targetMeshFilter.GetComponent<MeshRenderer>();
    }

    public override bool HasPreviewGUI()
    {
        ValidateData();

        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        _drag = Drag2D(_drag, r);

        if (Event.current.type == EventType.Repaint)
        {
            if(_targetMeshRenderer == null)
            {
                EditorGUI.DropShadowLabel(r, "Mesh Renderer Required");
            }
            else
            {
                _previewRenderUtility.BeginPreview(r, background);

                _previewRenderUtility.DrawMesh(_targetMeshFilter.sharedMesh, Matrix4x4.identity, _targetMeshRenderer.sharedMaterial, 0);

                _previewRenderUtility.m_Camera.transform.position = Vector2.zero;
                _previewRenderUtility.m_Camera.transform.rotation = Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
                _previewRenderUtility.m_Camera.transform.position = _previewRenderUtility.m_Camera.transform.forward * -6f;
                _previewRenderUtility.m_Camera.Render();

                Texture resultRender = _previewRenderUtility.EndPreview();
                GUI.DrawTexture(r, resultRender, ScaleMode.ScaleToFit, true);
            }

        }
    }

    public override void OnPreviewSettings()
    {
        if (GUILayout.Button("Reset Camera", EditorStyles.whiteMiniLabel))
            _drag = Vector2.zero;
    }

    void OnDestroy()
    {
        _previewRenderUtility.Cleanup();
    }

    public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
    {
        int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
        Event current = Event.current;
        switch (current.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (position.Contains(current.mousePosition) && position.width > 50f)
                {
                    GUIUtility.hotControl = controlID;
                    current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID)
                {
                    GUIUtility.hotControl = 0;
                }
                EditorGUIUtility.SetWantsMouseJumping(0);
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID)
                {
                    scrollPosition -= current.delta * (float)((!current.shift) ? 1 : 3) / Mathf.Min(position.width, position.height) * 140f;
                    scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
                    current.Use();
                    GUI.changed = true;
                }
                break;
        }
        return scrollPosition;
    }

}
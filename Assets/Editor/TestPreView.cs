using UnityEngine;
using UnityEditor;

public class GameObjectEditorWindow: EditorWindow {

    GameObject gameObject;
    GameObject oldgameObject;

    Editor gameObjectEditor;
    
    [MenuItem("Window/GameObject Editor")]
    static void ShowWindow() {
        GetWindow<GameObjectEditorWindow>("GameObject Editor");    
    }
    
    void OnGUI() {
        OnGui2();
    }

    private void OnGui1() {
        gameObject = (GameObject)EditorGUILayout.ObjectField(gameObject, typeof(GameObject), false);
        if (oldgameObject != gameObject)
        {
            oldgameObject = gameObject;
            gameObjectEditor = null;
        }

        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(300));
        if (gameObject != null)
        {
            if (gameObjectEditor == null)
                gameObjectEditor = Editor.CreateEditor(gameObject);

            gameObjectEditor.OnPreviewGUI(GUILayoutUtility.GetRect(300, 300), EditorStyles.whiteLabel);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void OnGui2() {
        gameObject = (GameObject)EditorGUILayout.ObjectField(gameObject, typeof(GameObject), false);
        if (oldgameObject != gameObject)
        {
            oldgameObject = gameObject;
  //          gameObjectEditor = null;

            if (_previewRenderUtility == null)
            {
                _previewRenderUtility = new PreviewRenderUtility();

                //We set the previews camera to 6 units back, look towards the middle of the 'scene'
                _previewRenderUtility.m_Camera.transform.position = new Vector3(0, 0, -6);
                _previewRenderUtility.m_Camera.transform.rotation = Quaternion.identity;
            }

            //We'll need the GO's mesh filter and renderer
            //to be able to render a preview of the mesh!
          //  _targetMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
          //  _targetMaterial = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
            if (_targetMesh == null && _targetMaterial == null)
            {
                _targetMesh = gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
                _targetMaterial = gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
            }
        }
        if (GUILayout.Button("Reset Camera", EditorStyles.whiteMiniLabel))
            _drag = Vector2.zero;

        if (gameObject != null && _previewRenderUtility != null && _targetMesh != null && _targetMaterial != null)
        {
            OnPreviewGUI(GUILayoutUtility.GetRect(300, 300), GUIStyle.none);

           // gameObjectEditor.OnPreviewGUI();
        }
    }

    public void OnPreviewGUI(Rect r, GUIStyle background)
    {
        _drag = Drag2D(_drag, r);

        //Only render our 3D 'preview' when the UI is 'repainting'.
        //The OnPreviewGUI, like other GUI methods, will be called LOTS
        //of times ever frame to handle different events.
        //We only need to Render our preview once when the GUI is being repainted!
        if (Event.current.type == EventType.Repaint)
        {
            //Tell the PRU to prepair itself - we pass along the
            //rect of the preview area so the PRU knows what size 
            //of a preview to render.
            _previewRenderUtility.BeginPreview(r, background);

            //We draw our mesh manually - it is not attached to any 'gameobject' in the preview 'scene'.
            //The preview 'scene' only contains a camera and a light. We need to render things manually.
            //We pass along the mesh set on the mesh filter and the material set on the renderer
            _previewRenderUtility.DrawMesh(_targetMesh, Matrix4x4.identity, _targetMaterial, 0);

            //Tell the camera to actually render the preview.
            _previewRenderUtility.m_Camera.transform.position = Vector2.zero;
            _previewRenderUtility.m_Camera.transform.rotation = Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
            _previewRenderUtility.m_Camera.transform.position = _previewRenderUtility.m_Camera.transform.forward * -6f;
            _previewRenderUtility.m_Camera.Render();

            //Now that we are done, we can end the preview. This method will spit out a Texture
            //The texture contains the image that was rendered by the preview utillity camera :)
            Texture resultRender = _previewRenderUtility.EndPreview();

            //If we omit the line bellow, then you wouldnt actually see anything in the preview!
            //The preview image is generated, but that was all done in our 'virtual' PreviewRenderUtility 'scene'.
            //We still need to draw something in the PreviewGUI area..!

            //So we draw the image that was generated into the preview GUI area, filling the entire area with this image.
            GUI.DrawTexture(r, resultRender, ScaleMode.ScaleToFit, true);
        }
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

    private PreviewRenderUtility _previewRenderUtility;
    private MeshFilter _targetMeshFilter;
    private MeshRenderer _targetMeshRenderer;

    private Mesh _targetMesh;
    private Material _targetMaterial;
    private Vector2 _drag;

}
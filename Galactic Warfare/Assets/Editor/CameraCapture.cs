using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CameraCapture : EditorWindow
{
	public RenderTexture TargetTexture = null;
	public Camera SnapshotCamera = null;
	public string SnapshotName = "";

	public SerializedObject SerializedSnapshotCreator = null;

	[MenuItem("Tools/CameraCapture")]
	public static void ShowWindow()
	{
		GetWindow<CameraCapture>("Take Snapshot");
	}

	private void OnEnable()
	{
		SerializedSnapshotCreator = new SerializedObject(this);
	}

	private void OnGUI()
	{
		EditorGUILayout.PropertyField(SerializedSnapshotCreator.FindProperty("TargetTexture"));
		EditorGUILayout.PropertyField(SerializedSnapshotCreator.FindProperty("SnapshotCamera"));
		EditorGUILayout.PropertyField(SerializedSnapshotCreator.FindProperty("SnapshotName"));

		SerializedSnapshotCreator.ApplyModifiedProperties();

		if (GUILayout.Button("Align Camera"))
		{

			Camera sceneCamera = SceneView.lastActiveSceneView.camera;
			SnapshotCamera.transform.SetPositionAndRotation(sceneCamera.transform.position, sceneCamera.transform.rotation);
		}

		if(GUILayout.Button("Take Snapshot"))
		{
			SnapshotCamera.Render();

			Texture2D textureToSave = ToTexture2D(ref TargetTexture);
			byte[] imageData = textureToSave.EncodeToPNG();
			DestroyImmediate(textureToSave);
			File.WriteAllBytes(GetSnapshotName(), imageData);

			AssetDatabase.Refresh();
		}
	}

	private Texture2D ToTexture2D(ref RenderTexture target)
	{
		Texture2D result = new Texture2D(target.width, target.height);
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = target;

		result.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
		result.Apply();

		RenderTexture.active = currentRT;
		return result;
	}

	string GetSnapshotName()
	{
		if(SnapshotName == null || SnapshotName == "")
		{
			return string.Format("{0}/Snapshots/snap_{1}.png",
			Application.dataPath,
			System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
		}
		return $"{Application.dataPath}/Snapshots/{SnapshotName}.png";
	}
}

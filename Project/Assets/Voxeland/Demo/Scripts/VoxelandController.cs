using UnityEngine;
using System.Collections;
using Voxeland5;

//using Plugins;

namespace VoxelandDemo
{

	public class VoxelandController : MonoBehaviour 
	{
		public Voxeland voxeland;
		public CameraController cameraController;
		public WalkController charController;
		public CoordDir prevAimCoord;

		//gui
		public Canvas canvas;
		public GameObject helpPanel;
		public GameObject crosshair;
		public UnityEngine.UI.Toggle useMouselook;
		public UnityEngine.UI.Toggle useGravity;
		public UnityEngine.UI.Toggle useFullscreen;
		public GameObject fullscreenCheckmark;
		private int oldScreenWidth = 1024;
		private int oldScreenHeight = 768;
		public UnityEngine.UI.Slider buildProgress;
		public GameObject autogeneratePleaseWait;

		//instrument selection
		public int selectedTool = 1;
		public UnityEngine.UI.Toggle vertGrassInstrument;
		public UnityEngine.UI.Toggle darkCliffInstrument;
		public UnityEngine.UI.Toggle brightCliffInstrument;
		public UnityEngine.UI.Toggle grassInstrument;
		public UnityEngine.UI.Toggle yellowGrassInstrument;
		public UnityEngine.UI.Toggle pineInstrument;
		public UnityEngine.UI.Toggle torchInstrument;
		public GameObject instrumentWarning;
		
		//disabling fullscreen and mouselook when loosing focus
		/*void OnApplicationFocus(bool focusStatus) 
		{
			if (!focusStatus && Screen.fullScreen)
			{
				useFullscreen.isOn = false;
				useMouselook.isOn = false;
			}
		}*/

		public void SwitchFullscreen ()
		{
			if (!Screen.fullScreen) 
			{ 
				if (oldScreenWidth == 0) oldScreenWidth = 1024;
				if (oldScreenHeight == 0) oldScreenHeight = 768;
				oldScreenWidth = Screen.width;
				oldScreenHeight = Screen.height;
				Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
			}
			else
				Screen.SetResolution(oldScreenWidth, oldScreenHeight, false);
		}

		void OnEnable () 
		{
			//select cliff tool
			voxeland.grassTypes.selected=-1;	voxeland.landTypes.selected=0;		voxeland.objectsTypes.selected=-1;
			canvas.gameObject.SetActive(true);
		}

		void Update ()
		{
			if (Input.GetKeyDown(KeyCode.Escape)) { Application.Quit(); }

			//showing help panel
			if (Input.GetKeyDown(KeyCode.F1)) helpPanel.SetActive( !helpPanel.activeSelf );

			//selecting tool
			if (Input.GetKey("`")) vertGrassInstrument.isOn = true;
			if (Input.GetKey("1")) brightCliffInstrument.isOn = true;
			if (Input.GetKey("2")) darkCliffInstrument.isOn = true;
			if (Input.GetKey("3")) grassInstrument.isOn = true;
			if (Input.GetKey("4")) yellowGrassInstrument.isOn = true;
			if (Input.GetKey("5")) pineInstrument.isOn = true;
			if (Input.GetKey("6")) torchInstrument.isOn = true;

			if (vertGrassInstrument.isOn) { voxeland.grassTypes.selected=0;		voxeland.landTypes.selected=-1;		voxeland.objectsTypes.selected=-1; }
			if (darkCliffInstrument.isOn) { voxeland.grassTypes.selected=-1;	voxeland.landTypes.selected=0;		voxeland.objectsTypes.selected=-1; }
			if (brightCliffInstrument.isOn) { voxeland.grassTypes.selected=-1;	voxeland.landTypes.selected=1;		voxeland.objectsTypes.selected=-1; }
			if (grassInstrument.isOn) { voxeland.grassTypes.selected=-1;	voxeland.landTypes.selected=2;		voxeland.objectsTypes.selected=-1; }
			if (yellowGrassInstrument.isOn) { voxeland.grassTypes.selected=-1;	voxeland.landTypes.selected=3;		voxeland.objectsTypes.selected=-1; }
			if (pineInstrument.isOn) { voxeland.grassTypes.selected=-1;	voxeland.landTypes.selected=-1;		voxeland.objectsTypes.selected=0; }
			if (torchInstrument.isOn) { voxeland.grassTypes.selected=-1;	voxeland.landTypes.selected=-1;		voxeland.objectsTypes.selected=1; }
			
			if (voxeland.landTypes.selected==-1) instrumentWarning.SetActive(true);
			else instrumentWarning.SetActive(false);


			//mouselook and gravity
			if (Input.GetKeyDown("g")) { useGravity.isOn = !useGravity.isOn; }
			if (Input.GetKeyDown("m")) { useMouselook.isOn = !useMouselook.isOn; }
			if (Input.GetKeyDown("f")) { SwitchFullscreen(); }
			//if (Input.GetKeyDown(KeyCode.Escape)) { useMouselook.isOn = false; useFullscreen.isOn = false; }

			charController.gravity = useGravity.isOn;
			cameraController.lockCursor = useMouselook.isOn;
			crosshair.SetActive(cameraController.lockCursor);

			//fullscreen - reverse order, setting toggle from current fullscreen state
			//useFullscreen.isOn = Screen.fullScreen;
			fullscreenCheckmark.SetActive(Screen.fullScreen);

			//displaing build progress
			float calculatedSum; float completeSum; float totalSum;
			ThreadWorker.GetProgresByTag("VoxelandChunk", out totalSum, out calculatedSum, out completeSum);
			buildProgress.maxValue = totalSum;
			buildProgress.value = completeSum;

			//editing
			if (cameraController.lockCursor || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) //IsPointerOverGameObject returns true if mouse hidden
			{
				//reading controls
				bool leftMouse = Input.GetMouseButtonDown(0);
				//bool middleMouse = Input.GetMouseButtonDown(2);
				bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
				//bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);  //not used
				bool control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

				//getting edit mode
				Voxeland.EditMode editMode = Voxeland.EditMode.none;
				if (leftMouse && !shift && !control) editMode = Voxeland.EditMode.dig;
				else if (leftMouse && control) editMode = Voxeland.EditMode.add;
				else if (leftMouse && shift) editMode = Voxeland.EditMode.replace;

				//getting aiming ray
				Ray aimRay;
				if (cameraController.lockCursor) aimRay = Camera.main.ViewportPointToRay(new Vector3(0.5f,0.5f,0.5f));
				else aimRay = Camera.main.ScreenPointToRay(Input.mousePosition);

				//aiming terrain block
				CoordDir aimCoord = voxeland.PointOut(aimRay);

				//if any change
				if (prevAimCoord != aimCoord || editMode != Voxeland.EditMode.none)
				{
					//highlight
					if (voxeland.highlight!=null)
					{
						if (aimCoord.exists) voxeland.Highlight(aimCoord, voxeland.brush, isEditing:editMode!=Voxeland.EditMode.none);
						else voxeland.highlight.Clear(); //clearing highlight if nothing aimed or voxeland not selected
					}

					//altering
					if (editMode!=Voxeland.EditMode.none && aimCoord.exists) 
					{
						voxeland.Alter(aimCoord, voxeland.brush, editMode, 
							landType:voxeland.landTypes.selected, 
							objectType:voxeland.objectsTypes.selected, 
							grassType:voxeland.grassTypes.selected);
					}

					prevAimCoord = aimCoord;
				} //if coord or button change
			}
		}

		public void SetSelectedTool (int tool) { selectedTool = tool; }
		public void SetMouseLook (bool val) { cameraController.lockCursor = val; }
		public void SetGravity (bool val) { cameraController.lockCursor = val; }

	}

}

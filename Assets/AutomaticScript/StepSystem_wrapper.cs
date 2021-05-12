using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class StepSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void autoExecuteStep(System.Boolean on)
	{
		MainLoop.callAppropriateSystemMethod ("StepSystem", "autoExecuteStep", on);
	}

	public void goToNextStep()
	{
		MainLoop.callAppropriateSystemMethod ("StepSystem", "goToNextStep", null);
	}

}

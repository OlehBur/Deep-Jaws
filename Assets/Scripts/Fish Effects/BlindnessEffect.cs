public class BlindnessEffect : Effect {


    protected override void OnApply() {
        //Debug.Log($"{target.name} has applied Blindness Effect for {duration} s.");
    }

    protected override void OnExpire() {
        //Debug.Log($"{target.name} has expired Blindness Effect after {duration} s.");
    }

    //new void Update() {
    //    base.Update();

    //}
}

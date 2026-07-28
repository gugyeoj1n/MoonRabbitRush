using MoonRabbitRush;
using UnityEngine;

public class TestBinding : MonoBehaviour
{

    public void OnClickTest()
    {
        DataBindingManager.AddValue(Property.PlayerHealth, -1);
    }
}

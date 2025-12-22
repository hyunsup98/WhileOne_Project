using UnityEngine;

public class AfterimagePool : MonoBehaviour
{
    static ObjectPools<Blink> objPool = new ObjectPools<Blink>();
    [SerializeField] float size = 4;
    [SerializeField] Blink _blink;
        
    private void Awake()
    {

        for(int i = 0; i< size; i++)
        {
            //objPool.GetObject(_blink, gameObject.transform);
            objPool.TakeObject(_blink);
        }

    }
    public void Take()
    {
        //objPool.TakeObject(_blink);
        objPool.GetObject(_blink, gameObject.transform);

    }



}

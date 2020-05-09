using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;


public class ScrollBarButton : MonoBehaviour
{
    [SerializeField]
    private ScrollRect target = null;

    public int scrollSpeed = 1;
    public RectTransform block = null;
    public float blockSize = 50f;
    public bool isScrollingDown = true;
    
    public void Scroll()
    {
        
        if (target == null)
        {
            Debug.LogError(transform.parent.parent.name + "/" + transform.parent.name + "/" + gameObject.name+": No target ScrollRect acquired");
            return;
        }

        if (block != null)
            blockSize = block.rect.height;

        float space = target.content.gameObject.GetComponent<VerticalLayoutGroup>().spacing;
        //Debug.Log("space=" + space.ToString());
        int blockCount = (int)Mathf.Floor(target.content.rect.height / (blockSize + space));
        //Debug.Log("blockCount=" + blockCount.ToString());
        float delta = (float)scrollSpeed / blockCount + space / target.content.rect.height;
        //Debug.Log("delta=" + delta.ToString());


        if (isScrollingDown)
        {
            target.verticalNormalizedPosition -= delta;
        }
        else
        {
            target.verticalNormalizedPosition += delta;
        }

        if (target.verticalNormalizedPosition > 1.0f)
            target.verticalNormalizedPosition = 1.0f;

        if (target.verticalNormalizedPosition < 0f)
            target.verticalNormalizedPosition = 0f;


    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// A horizontal ScrollRect that lives inside the shop's vertical scroll. Stock nested
    /// ScrollRects swallow every drag; this one routes a drag to whichever axis dominates its
    /// first movement — horizontal stays here, vertical is forwarded to the parent scroll.
    /// </summary>
    public class ShopRowScroll : ScrollRect
    {
        private ScrollRect _parentScroll;
        private bool _routeToParent;

        protected override void Awake()
        {
            base.Awake();
            for (Transform t = transform.parent; t != null && _parentScroll == null; t = t.parent)
                _parentScroll = t.GetComponent<ScrollRect>();
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            _parentScroll?.OnInitializePotentialDrag(eventData);
            base.OnInitializePotentialDrag(eventData);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            _routeToParent = Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x);
            if (_routeToParent) _parentScroll?.OnBeginDrag(eventData);
            else base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (_routeToParent) _parentScroll?.OnDrag(eventData);
            else base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_routeToParent) _parentScroll?.OnEndDrag(eventData);
            else base.OnEndDrag(eventData);
            _routeToParent = false;
        }
    }
}

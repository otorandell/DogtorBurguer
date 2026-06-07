using UnityEngine;

namespace DogtorBurguer
{
    public class GameLayout : MonoBehaviour
    {
        [Header("Grid Panel")]
        [SerializeField] private Vector2 _gridPanelCenter = UIStyles.GRID_PANEL_CENTER;
        [SerializeField] private Vector2 _gridPanelSize = UIStyles.GRID_PANEL_SIZE;

        [Header("Top Left Panel")]
        [SerializeField] private Vector2 _topLeftCenter = UIStyles.TOP_LEFT_PANEL_CENTER;
        [SerializeField] private Vector2 _topLeftSize = UIStyles.TOP_LEFT_PANEL_SIZE;

        [Header("Top Right Panel")]
        [SerializeField] private Vector2 _topRightCenter = UIStyles.TOP_RIGHT_PANEL_CENTER;
        [SerializeField] private Vector2 _topRightSize = UIStyles.TOP_RIGHT_PANEL_SIZE;

        [Header("Border Style")]
        [SerializeField] private Color _borderColor = UIStyles.LAYOUT_BORDER;
        [SerializeField] private Color _fillColor = UIStyles.LAYOUT_FILL;
        [SerializeField] private int _borderWidth = UIStyles.PANEL_BORDER_WIDTH;
        [SerializeField] private int _cornerRadius = UIStyles.PANEL_CORNER_RADIUS;

        private const int TEX_SIZE = 128;
        private Sprite _panelSprite;

        private void Start()
        {
            _panelSprite = SpriteFactory.RoundedPanel(_borderColor, _fillColor, _cornerRadius, _borderWidth, TEX_SIZE);

            CreatePanel("GridPanel", _gridPanelCenter, _gridPanelSize);
            CreatePanel("TopLeftPanel", _topLeftCenter, _topLeftSize);
            CreatePanel("TopRightPanel", _topRightCenter, _topRightSize);
        }

        private void CreatePanel(string name, Vector2 center, Vector2 size)
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(transform, false);
            panelObj.transform.position = new Vector3(center.x, center.y, Constants.Z_GAME_PANEL);

            SpriteRenderer sr = panelObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = Constants.SORT_GAME_PANEL;
            sr.sprite = _panelSprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
        }
    }
}

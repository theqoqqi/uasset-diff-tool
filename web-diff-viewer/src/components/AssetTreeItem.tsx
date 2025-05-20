import React, {useState} from 'react';
import styles from './AssetTreeItem.module.css';
import type { AssetTreeNode } from '../utils/tree';
import { sortTreeNodes } from '../utils/tree';
import type { AssetDiff, DiffType } from '../diffs/types';

interface AssetTreeItemProps {
    item: AssetTreeNode;
    onSelect: (asset: AssetDiff) => void;
}

const AssetTreeItem: React.FC<AssetTreeItemProps> = ({ item, onSelect }) => {
    const [open, setOpen] = useState(false);
    const statusClass = item.status.toLowerCase() as Lowercase<DiffType>;
    const isFolder = !item.asset && item.children.length > 0;
    const isAsset = !!item.asset;

    function handleClick() {
        if (item.asset) {
            onSelect(item.asset);
        } else if (isFolder) {
            setOpen(!open);
        }
    }

    return (
        <div className={styles.item}>
            <div
                className={`${styles.header} ${styles[statusClass]}`}
                onClick={handleClick}
            >
                <span className={styles.icon}>
                    {isFolder && (
                        <span className={`${styles.folderIcon} ${open ? styles.open : ''}`}>
                            {open ? '▼' : '▶'}
                        </span>
                    )}
                    {isAsset && (
                        <span className={styles.assetIcon}>📄</span>
                    )}
                </span>
                <span className={styles.label}>
                    {item.name}
                </span>
            </div>
            {isFolder && open && (
                <div className={styles.children}>
                    {sortTreeNodes(item.children).map(child => (
                        <AssetTreeItem
                            key={child.fullName}
                            item={child}
                            onSelect={onSelect}
                        />
                    ))}
                </div>
            )}
        </div>
    );
};

export default AssetTreeItem;

import React, { useState } from 'react';
import styles from './AssetTreeItem.module.css';
import type { AssetTreeNode } from '../utils/tree';
import type {AssetDiff, DiffType} from '../diffs/types';

interface AssetTreeItemProps {
    item: AssetTreeNode;
    onSelect: (asset: AssetDiff) => void;
}

const AssetTreeItem: React.FC<AssetTreeItemProps> = ({ item, onSelect }) => {
    const [open, setOpen] = useState(false);
    const statusClass = item.status.toLowerCase() as Lowercase<DiffType>;

    function handleClick() {
        if (item.asset) {
            onSelect(item.asset);
        } else {
            setOpen(!open);
        }
    }

    return (
        <div className={styles.item}>
            <div
                className={`${styles.label} ${styles[statusClass]}`}
                onClick={handleClick}
            >
                {item.name}
            </div>
            {item.children.length > 0 && open && (
                <div className={styles.children}>
                    {item.children.map(child => (
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

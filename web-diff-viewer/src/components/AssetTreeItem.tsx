import React, {useState} from 'react';
import styles from './AssetTreeItem.module.css';
import type { AssetTreeNode } from '../utils/tree';
import { sortTreeNodes } from '../utils/tree';
import type { AssetDiff, DiffType } from '../diffs/types';

interface AssetTreeItemProps {
    item: AssetTreeNode;
    onSelect: (asset: AssetDiff) => void;
}

interface ChangeSummaryProps {
    node: AssetTreeNode;
}

const ChangeSummary: React.FC<ChangeSummaryProps> = ({ node }) => {
    function count(node: AssetTreeNode): Record<DiffType, number> {
        const totals: Record<DiffType, number> = {
            Added: 0,
            Removed: 0,
            Changed: 0,
            Unchanged: 0,
        };

        if (node.asset) {
            totals[node.asset.DiffType] += 1;
        }

        for (const child of node.children) {
            const childTotals = count(child);

            (Object.keys(childTotals) as DiffType[]).forEach(key => {
                totals[key] += childTotals[key];
            });
        }

        return totals;
    }

    const {
        Added: added,
        Removed: removed,
        Changed: changed,
    } = count(node);

    if (added + removed + changed === 0) {
        return null;
    }

    return (
        <span className={styles.summary}>
            {added > 0 && <span className={styles.added}>+{added}</span>}
            {removed > 0 && <span className={styles.removed}>-{removed}</span>}
            {changed > 0 && <span className={styles.changed}>~{changed}</span>}
        </span>
    );
};

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
                {isFolder && <ChangeSummary node={item} />}
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

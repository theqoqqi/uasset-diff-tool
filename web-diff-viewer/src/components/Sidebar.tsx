import React from 'react';
import AssetTreeItem from './AssetTreeItem';
import { buildAssetTree } from '../utils/tree';
import styles from './Sidebar.module.css';
import type { AssetDiff } from '../diffs/types';

interface SidebarProps {
    data: Record<string, AssetDiff>;
    onSelect: (asset: AssetDiff) => void;
}

const Sidebar: React.FC<SidebarProps> = ({ data, onSelect }) => {
    const assets = Object.values(data);
    const tree = buildAssetTree(assets);

    return (
        <aside className={styles.sidebar}>
            {tree.map(node => (
                <AssetTreeItem
                    key={node.fullName}
                    item={node}
                    onSelect={onSelect}
                />
            ))}
        </aside>
    );
};

export default Sidebar;

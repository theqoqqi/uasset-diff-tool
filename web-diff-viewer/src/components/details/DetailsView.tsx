import React from 'react';
import FunctionDiffCard from './FunctionDiffCard';
import PropertyDiffCard from './PropertyDiffCard';
import styles from './DetailsView.module.css';
import type { AssetDiff, PropertyDiff, FunctionDiff } from '../../diffs/types';

interface DetailsViewProps {
    asset: AssetDiff | null;
}

const DetailsView: React.FC<DetailsViewProps> = ({ asset }) => {
    if (!asset) {
        return (
            <div className={styles.container}>
                Select asset
            </div>
        );
    }

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h2>{asset.Name}</h2>
                <p>Status: {asset.DiffType}</p>
                <p>Path: {asset.Path?.From || ''} → {asset.Path?.To || ''}</p>
            </div>

            {Object.entries(asset.Properties ?? {}).map(([name, diff]) => (
                <PropertyDiffCard
                    key={name}
                    name={name}
                    diff={diff}
                />
            ))}

            {Object.entries(asset.Functions ?? {}).map(([name, diff]) => (
                <FunctionDiffCard
                    key={name}
                    name={name}
                    diff={diff}
                />
            ))}
        </div>
    );
};

export default DetailsView;

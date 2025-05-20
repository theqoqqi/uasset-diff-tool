import React from 'react';
import styles from './DetailsView.module.css';
import type {AssetDiff} from '../../diffs/types';
import PropertyDiffList from './PropertyDiffList';
import FunctionDiffList from './FunctionDiffList';
import ValueChangeView from './ValueChangeView';

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
                <ValueChangeView label='Path' change={asset.Path} />
            </div>
            <PropertyDiffList properties={asset.Properties} />
            <FunctionDiffList functions={asset.Functions} />
        </div>
    );
};

export default DetailsView;

import React from 'react';
import styles from './ValueChangeView.module.css';
import type {ValueChange} from '../../diffs/types';
import {DiffType} from '../../diffs/types';

interface ValueChangeViewProps {
    label: string;
    change?: ValueChange<any>;
    className?: string;
}

const ValueChangeView: React.FC<ValueChangeViewProps> = ({
    label,
    change,
    className = ''
}) => {
    if (!change) {
        return null;
    }

    const fromValue = change.From ?? '';
    const toValue = change.To ?? '';
    const hasChanges = change.DiffType !== DiffType.Unchanged;

    return (
        <div className={`${styles.view} ${className} ${hasChanges ? styles.changed : ''}`}>
            <span className={styles.label}>{label}:</span>
            <span className={styles.from}>{fromValue}</span>
            <span className={styles.arrow}>→</span>
            <span className={styles.to}>{toValue}</span>
        </div>
    );
};

export default ValueChangeView;

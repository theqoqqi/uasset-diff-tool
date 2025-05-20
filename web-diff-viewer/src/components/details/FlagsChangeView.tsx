import React from 'react';
import type { FlagsChange } from '../../diffs/types';
import ExpandableCard from './ExpandableCard';
import styles from './FlagsChangeView.module.css';

interface FlagsChangeViewProps {
    label: string;
    change?: FlagsChange<string>;
}

interface FlagListProps {
    label: string;
    flags: Set<string>;
    variant: 'added' | 'removed';
}

const FlagList: React.FC<FlagListProps> = ({ label, flags, variant }) => {
    if (flags.size === 0) return null;

    return (
        <div className={styles.flagListField}>
            <span>{label}:</span>
            <div className={styles.flagList}>
                {Array.from(flags).map(flag => (
                    <span
                        key={flag}
                        className={`${styles.flag} ${
                            variant === 'added' ? styles.added : styles.removed
                        }`}
                    >
            {flag}
          </span>
                ))}
            </div>
        </div>
    );
};

const FlagsChangeView: React.FC<FlagsChangeViewProps> = ({ label, change }) => {
    if (!change) return null;

    const parseFlags = (flags: string) =>
        new Set(
            flags
                .split(', ')
                .filter(Boolean)
                .filter(flag => !flag.endsWith('_None'))
        );

    const addedFlags = parseFlags(change.Added ?? '');
    const removedFlags = parseFlags(change.Removed ?? '');

    return (
        <ExpandableCard title={label} status={change.DiffType}>
            <div className={styles.flagListsContainer}>
                <FlagList label="Added" flags={addedFlags} variant="added" />
                <FlagList label="Removed" flags={removedFlags} variant="removed" />
            </div>
        </ExpandableCard>
    );
};

export default FlagsChangeView;

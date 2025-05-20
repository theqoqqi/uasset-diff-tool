import React, { useState } from 'react';
import styles from './ExpandableCard.module.css';
import type { DiffType } from '../../diffs/types';

interface ExpandableCardProps {
    title: string;
    status?: DiffType;
    children: React.ReactNode;
}

const ExpandableCard: React.FC<ExpandableCardProps> = ({
    title,
    status,
    children
}) => {
    const [open, setOpen] = useState(false);

    const toggleOpen = () => setOpen(!open);

    return (
        <div className={styles.wrapper}>
            <div className={styles.summary} onClick={toggleOpen}>
                {title} {status && `(${status})`}
            </div>
            {open && (
                <div className={styles.content}>
                    {children}
                </div>
            )}
        </div>
    );
};

export default ExpandableCard;

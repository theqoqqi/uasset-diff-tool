import React, { useState } from 'react';
import styles from './ExpandableCard.module.css';
import type { DiffType } from '../../diffs/types';

interface ExpandableCardProps {
    title: string;
    status?: DiffType;
    children: React.ReactNode;
    contentStyle?: React.CSSProperties;
}

const ExpandableCard: React.FC<ExpandableCardProps> = ({
    title,
    status,
    children,
    contentStyle,
}) => {
    const [open, setOpen] = useState(false);

    const toggleOpen = () => setOpen(!open);

    const getStatusClass = (status?: DiffType) => {
        if (!status) {
            return '';
        }

        return styles[status.toLowerCase() as Lowercase<DiffType>];
    };

    return (
        <div className={styles.wrapper}>
            <div
                className={`${styles.summary} ${getStatusClass(status)}`}
                onClick={toggleOpen}
            >
                {title} {status && `(${status})`}
            </div>
            {open && (
                <div className={styles.content} style={contentStyle}>
                    {children}
                </div>
            )}
        </div>
    );
};

export default ExpandableCard;
